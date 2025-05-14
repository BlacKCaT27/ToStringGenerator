using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Bcss.ToStringGenerator.Generators
{
    [Generator]
    public class ClassToStringGenerator : IIncrementalGenerator
    {
        private const string DefaultRedactionValue = "[REDACTED]";
        private const string ConfigurationKey = "build_property.ToStringGeneratorRedactedValue";
        private const string GenerateToStringAttributeName = "Bcss.ToStringGenerator.Attributes.GenerateToStringAttribute";
        private const string SensitiveDataAttributeName = "Bcss.ToStringGenerator.Attributes.SensitiveDataAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var defaultRedactionConfig = GetDefaultRedactionConfig(context);
            var typeDeclarations = GetTypeDeclarations(context);
            var combined = CombineProviders(typeDeclarations, defaultRedactionConfig);

            context.RegisterSourceOutput(
                combined.Combine(context.CompilationProvider),
                (spc, tuple) => Execute(spc, tuple.Left.DefaultRedaction ?? string.Empty, tuple.Left.Type, tuple.Right!));
        }

        private static IncrementalValueProvider<string> GetDefaultRedactionConfig(IncrementalGeneratorInitializationContext context)
        {
            return context.AnalyzerConfigOptionsProvider
                .Select((provider, _) =>
                {
                    if (provider.GlobalOptions.TryGetValue(ConfigurationKey, out var value))
                    {   
                        return value;
                    }

                    return DefaultRedactionValue;
                });
        }

        private static IncrementalValuesProvider<TypeDeclarationSyntax?> GetTypeDeclarations(IncrementalGeneratorInitializationContext context)
        {
            return context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => GetTypeWithGenerateToStringAttribute(ctx));
        }

        private static TypeDeclarationSyntax? GetTypeWithGenerateToStringAttribute(GeneratorSyntaxContext ctx)
        {
            if (ctx.Node is not ClassDeclarationSyntax typeDeclaration) return null;

            var semanticModel = ctx.SemanticModel;
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (typeSymbol == null) return null;

            return typeSymbol.GetAttributes()
                .Any(attr => attr.AttributeClass?.ToDisplayString() == GenerateToStringAttributeName)
                ? typeDeclaration
                : null;
        }

        private static IncrementalValueProvider<(TypeDeclarationSyntax? Type, string DefaultRedaction)> CombineProviders(
            IncrementalValuesProvider<TypeDeclarationSyntax?> typeDeclarations,
            IncrementalValueProvider<string> defaultRedactionConfig)
        {
            return typeDeclarations
                .Collect()
                .Select((nodes, _) => nodes[0])
                .Combine(defaultRedactionConfig);
        }

        private static void Execute(SourceProductionContext context, string defaultRedactionValue, TypeDeclarationSyntax? typeDeclaration, Compilation compilation)
        {
            if (typeDeclaration == null) return;

            var semanticModel = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);

            if (typeSymbol == null) return;

            var sourceCode = GenerateToStringMethod(typeSymbol, defaultRedactionValue, compilation);
            var fileName = $"{typeSymbol.Name}.ToString.g.cs";

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(fileName, sourceCode);
        }

        private static string GenerateToStringMethod(ISymbol typeSymbol, string defaultRedactionValue, Compilation compilation)
        {
            var sourceBuilder = new StringBuilder();
            AddUsingsAndNamespace(sourceBuilder, typeSymbol);
            AddTypeDeclaration(sourceBuilder, typeSymbol);
            AddToStringMethod(sourceBuilder, typeSymbol, defaultRedactionValue, compilation);
            return sourceBuilder.ToString();
        }

        private static void AddUsingsAndNamespace(StringBuilder sourceBuilder, ISymbol typeSymbol)
        {
            sourceBuilder.AppendLine("using System;");
            sourceBuilder.AppendLine("using System.Text;");
            sourceBuilder.AppendLine("using System.Collections.Generic;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"namespace {typeSymbol.ContainingNamespace};");
            sourceBuilder.AppendLine();
        }

        private static void AddTypeDeclaration(StringBuilder sourceBuilder, ISymbol typeSymbol)
        {
            var accessModifier = typeSymbol.DeclaredAccessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => "private"
            };
            sourceBuilder.AppendLine($"{accessModifier} partial class {typeSymbol.Name}");
            sourceBuilder.AppendLine("{");
        }

        private static void AddToStringMethod(StringBuilder sourceBuilder, ISymbol typeSymbol, string defaultRedactionValue, Compilation compilation)
        {
            var accessModifier = typeSymbol.DeclaredAccessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => "private"
            };

            sourceBuilder.AppendLine($"    {accessModifier} override string ToString()");
            sourceBuilder.AppendLine("    {");
            sourceBuilder.AppendLine("        var sb = new StringBuilder();");
            sourceBuilder.AppendLine($"        sb.Append(\"[{typeSymbol.Name}: \");");
            sourceBuilder.AppendLine();

            var members = GetPublicMembers(typeSymbol);
            AppendMembers(sourceBuilder, members, defaultRedactionValue, compilation);

            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("        sb.Append(\"]\");");
            sourceBuilder.AppendLine("        return sb.ToString();");
            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");
        }

        private static IEnumerable<ISymbol> GetPublicMembers(ISymbol typeSymbol)
        {
            return ((INamespaceOrTypeSymbol)typeSymbol).GetMembers()
                .Where(m => (m.Kind == SymbolKind.Property || m.Kind == SymbolKind.Field) &&
                           m.DeclaredAccessibility == Accessibility.Public &&
                           !m.IsStatic);
        }

        private static void AppendMembers(StringBuilder sourceBuilder, IEnumerable<ISymbol> members, string defaultRedactionValue, Compilation compilation)
        {
            var firstMember = true;
            foreach (var member in members)
            {
                var memberName = member.Name;
                var separator = firstMember ? "" : ", ";
                sourceBuilder.AppendLine($"        sb.Append(\"{separator}{memberName} = \");");

                var sensitiveDataAttr = member.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == SensitiveDataAttributeName);

                if (sensitiveDataAttr == null)
                {
                    var memberType = GetMemberType(member);
                    if (IsDictionary(memberType))
                    {
                        AppendDictionaryValue(sourceBuilder, memberName, memberType);
                    }
                    else if (IsEnumerable(memberType, compilation))
                    {
                        AppendEnumerableValue(sourceBuilder, memberName, memberType);
                    }
                    else
                    {
                        if (IsNullableType(memberType))
                        {
                            sourceBuilder.AppendLine($"        if ({memberName} == null)");
                            sourceBuilder.AppendLine("        {");
                            sourceBuilder.AppendLine("            sb.Append(\"null\");");
                            sourceBuilder.AppendLine("        }");
                            sourceBuilder.AppendLine("        else");
                            sourceBuilder.AppendLine("        {");
                            sourceBuilder.AppendLine($"            sb.Append({memberName}.ToString());");
                            sourceBuilder.AppendLine("        }");
                        }
                        else
                        {
                            sourceBuilder.AppendLine($"        sb.Append({memberName}.ToString());");
                        }
                    }
                }
                else
                {
                    var redactionValue = GetRedactionValue(member, defaultRedactionValue);
                    sourceBuilder.AppendLine($"        sb.Append(\"{redactionValue}\");");
                }
                
                firstMember = false;
            }
        }

        private static ITypeSymbol GetMemberType(ISymbol member)
        {
            return member switch
            {
                IPropertySymbol property => property.Type,
                IFieldSymbol field => field.Type,
                _ => throw new ArgumentException($"Unexpected member type: {member.GetType()}")
            };
        }

        private static bool IsDictionary(ITypeSymbol type)
        {
            // Get the metadata name which includes the generic type parameters
            var metadataName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            
            // Check if it's a Dictionary<TKey,TValue> or IDictionary<TKey,TValue>
            return metadataName.StartsWith("Dictionary<", StringComparison.Ordinal) ||
                   metadataName.StartsWith("IDictionary<", StringComparison.Ordinal) ||
                   type.AllInterfaces.Any(i => 
                       i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        .StartsWith("IDictionary<", StringComparison.Ordinal));
        }

        private static bool IsEnumerable(ITypeSymbol type, Compilation compilation)
        {
            // Don't treat string as an enumerable
            if (type.SpecialType == SpecialType.System_String)
                return false;

            // Check for both generic and non-generic IEnumerable
            var genericEnumerable = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            var nonGenericEnumerable = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
            
            if (genericEnumerable == null || nonGenericEnumerable == null) 
                return false;

            // Check if the type implements IEnumerable<T> or IEnumerable
            return type.AllInterfaces.Any(i => 
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, genericEnumerable) ||
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, nonGenericEnumerable));
        }

        private static bool IsNullableType(ITypeSymbol type)
        {
            // Check if it's a nullable value type (e.g., int?)
            if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                return true;
            }

            // Check if it's a reference type with nullable annotation
            return type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.Annotated;
        }

        private static void AppendDictionaryValue(StringBuilder sourceBuilder, string memberName, ITypeSymbol type)
        {
            if (IsNullableType(type))
            {
                sourceBuilder.AppendLine($"        if ({memberName} == null)");
                sourceBuilder.AppendLine("        {");
                sourceBuilder.AppendLine("            sb.Append(\"null\");");
                sourceBuilder.AppendLine("        }");
                sourceBuilder.AppendLine("        else");
                sourceBuilder.AppendLine("        {");
                AppendDictionaryContents(sourceBuilder, memberName);
                sourceBuilder.AppendLine("        }");
            }
            else
            {
                AppendDictionaryContents(sourceBuilder, memberName);
            }
        }

        private static void AppendDictionaryContents(StringBuilder sourceBuilder, string memberName)
        {
            sourceBuilder.AppendLine("            sb.Append('[');");
            sourceBuilder.AppendLine($"            var {memberName}Enumerator = {memberName}.GetEnumerator();");
            sourceBuilder.AppendLine($"            if ({memberName}Enumerator.MoveNext())");
            sourceBuilder.AppendLine("            {");
            sourceBuilder.AppendLine($"                var pair = {memberName}Enumerator.Current;");
            sourceBuilder.AppendLine("                sb.Append('{');");
            sourceBuilder.AppendLine("                sb.Append(pair.Key.ToString());");
            sourceBuilder.AppendLine("                sb.Append(\" = \");");
            sourceBuilder.AppendLine("                sb.Append(pair.Value.ToString());");
            sourceBuilder.AppendLine("                sb.Append('}');");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"                while ({memberName}Enumerator.MoveNext())");
            sourceBuilder.AppendLine("                {");
            sourceBuilder.AppendLine("                    sb.Append(\", \");");
            sourceBuilder.AppendLine($"                    pair = {memberName}Enumerator.Current;");
            sourceBuilder.AppendLine("                    sb.Append('{');");
            sourceBuilder.AppendLine("                    sb.Append(pair.Key.ToString());");
            sourceBuilder.AppendLine("                    sb.Append(\" = \");");
            sourceBuilder.AppendLine("                    sb.Append(pair.Value.ToString());");
            sourceBuilder.AppendLine("                    sb.Append('}');");
            sourceBuilder.AppendLine("                }");
            sourceBuilder.AppendLine("            }");
            sourceBuilder.AppendLine("            sb.Append(']');");
        }

        private static void AppendEnumerableValue(StringBuilder sourceBuilder, string memberName, ITypeSymbol type)
        {
            if (IsNullableType(type))
            {
                sourceBuilder.AppendLine($"        if ({memberName} == null)");
                sourceBuilder.AppendLine("        {");
                sourceBuilder.AppendLine("            sb.Append(\"null\");");
                sourceBuilder.AppendLine("        }");
                sourceBuilder.AppendLine("        else");
                sourceBuilder.AppendLine("        {");
                AppendEnumerableContents(sourceBuilder, memberName);
                sourceBuilder.AppendLine("        }");
            }
            else
            {
                AppendEnumerableContents(sourceBuilder, memberName);
            }
        }

        private static void AppendEnumerableContents(StringBuilder sourceBuilder, string memberName)
        {
            sourceBuilder.AppendLine("            sb.Append('[');");
            sourceBuilder.AppendLine($"            var {memberName}Enumerator = {memberName}.GetEnumerator();");
            sourceBuilder.AppendLine($"            if ({memberName}Enumerator.MoveNext())");
            sourceBuilder.AppendLine("            {");
            sourceBuilder.AppendLine($"                sb.Append({memberName}Enumerator.Current.ToString());");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"                while ({memberName}Enumerator.MoveNext())");
            sourceBuilder.AppendLine("                {");
            sourceBuilder.AppendLine("                    sb.Append(\", \");");
            sourceBuilder.AppendLine($"                    sb.Append({memberName}Enumerator.Current.ToString());");
            sourceBuilder.AppendLine("                }");
            sourceBuilder.AppendLine("            }");
            sourceBuilder.AppendLine("            sb.Append(']');");
        }

        private static string GetRedactionValue(ISymbol member, string defaultRedactionValue)
        {
            var attributeSyntax = member.DeclaringSyntaxReferences
                .SelectMany(r => r.GetSyntax().DescendantNodes())
                .OfType<AttributeSyntax>()
                .FirstOrDefault(a => a.Name.ToString().Contains("SensitiveData"));

            if (attributeSyntax?.ArgumentList?.Arguments.Count > 0)
            {
                var arg = attributeSyntax.ArgumentList.Arguments[0];
                if (arg.Expression is LiteralExpressionSyntax literal)
                {
                    return literal.Token.ValueText;
                }
            }

            return defaultRedactionValue;
        }
    }
}
