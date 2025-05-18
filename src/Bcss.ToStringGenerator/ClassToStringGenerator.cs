using Microsoft.CodeAnalysis;
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
            GenerateMarkerAttributes(context);
            var defaultRedactionConfig = GetDefaultRedactionConfig(context);
            var typeDeclarations = GetTypeDeclarations(context);
            var combined = CombineProviders(typeDeclarations, defaultRedactionConfig);
            
            context.RegisterSourceOutput(
                combined.Combine(context.CompilationProvider),
                (spc, tuple) => Execute(spc, tuple.Left.DefaultRedaction ?? string.Empty, tuple.Left.Type));
        }

        private static void GenerateMarkerAttributes(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterPostInitializationOutput(i =>
            {
                const string generateToStringAttribute = @"
#if !TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES
namespace Bcss.ToStringGenerator.Attributes
{
    /// <summary>
    /// <p>Generates a ToString() method for the marked class or struct at compile time.</p>
    /// <p>By default, the string will be in the format:</p>
    /// <code>[className: member1Name = member1value, member2Name = member2value, ... ]</code>
    /// <br />
    /// <p>Collection members that implement IEnumerable or IEnumerableT will have each element written in square brackets, comma separated.</p>
    /// <code>[className: collectionMember = [value1, value2, value3] ... ]</code>
    /// <br />
    /// <p>Dictionary members that implement IDictionary or DictionaryT1, T2 will have each key-value pair written in brackets, comma separated.</p>
    /// <code>[className: dictionaryMember = [{key1 = value1}, {key2 = value2}] ... ]</code>
    /// <br />
    /// </summary>
    /// <remarks>
    /// <p>This attribute will be automatically loaded at compile time by the ToString source generator. You should not need to reference
    /// the project containing this attribute directly.</p>
    /// <br />
    /// <p>If your project exposes internal classes via [InternalsVisibleTo] and you reference the source generator package in multiple
    /// projects in one solution, you may end up with duplicate class definitions due to multiple generators being invoked. If this occurs,
    /// define the following constant in your projects .csproj file, then add a direct reference to the <c>Bcss.ToStringGenerator.Attributes</c>
    /// nuget package.</p>
    /// <br />
    /// <code><DefineConstants>TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES</DefineConstants></code>
    /// <br />
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class GenerateToStringAttribute : Attribute
    {
    }
}
#endif";
                i.AddSource("GenerateToStringAttribute.g.cs", generateToStringAttribute);
            });
            
            context.RegisterPostInitializationOutput(i =>
            {
                const string generateToStringAttribute = @"
#if !TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES
namespace Bcss.ToStringGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SensitiveDataAttribute : Attribute
    {
        public string MaskingValue { get; }

        /// <summary>
        /// Masks the value of the field or property that is marked with this attribute when
        /// generating a ToString() method.
        /// </summary>
        /// <remarks>
        /// <p>This attribute will be automatically loaded at compile time by the ToString source generator. You should not need to reference
        /// the project containing this attribute directly.</p>
        /// <br />
        /// <p>If your project exposes internal classes via [InternalsVisibleTo] and you reference the source generator package in multiple
        /// projects in one solution, you may end up with duplicate class definitions due to multiple generators being invoked. If this occurs,
        /// define the following constant in your projects .csproj file, then add a direct reference to the <c>Bcss.ToStringGenerator.Attributes</c>
        /// nuget package.</p>
        /// <br />
        /// <code><DefineConstants>TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES</DefineConstants></code>
        /// <br />
        /// </remarks>
        /// <param name=""maskingValue"">Sets the value that will be used in the ToString output instead of the members actual value.</param>
        public SensitiveDataAttribute(string maskingValue = ""[REDACTED]"")
        {
            MaskingValue = maskingValue;
        }
    }
}
#endif";
                i.AddSource("SensitiveDataAttribute.g.cs", generateToStringAttribute);
            });
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

        private static IncrementalValuesProvider<ClassSymbolData?> GetTypeDeclarations(IncrementalGeneratorInitializationContext context)
        {
            return context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    GenerateToStringAttributeName,
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => GetTypeWithGenerateToStringAttribute(ctx));
        }

        private static ClassSymbolData? GetTypeWithGenerateToStringAttribute(GeneratorAttributeSyntaxContext ctx)
        {
            var targetNode = ctx.Attributes
                .Any(attr => attr.AttributeClass?.ToDisplayString() == GenerateToStringAttributeName)
                ? ctx.TargetNode
                : null;

            if (targetNode is null) return null;

            var typeSymbol = ctx.SemanticModel.GetDeclaredSymbol(targetNode);
            if (typeSymbol is null) return null;
            
            string containingNamespace = typeSymbol.ContainingNamespace.ToDisplayString();
            string classAccessibility = GetAccessibility(typeSymbol);
            string className = typeSymbol.Name;
            var memberSymbols = GetPublicMembers(typeSymbol);
            var members = GetMemberSymbolData(memberSymbols, ctx.SemanticModel.Compilation);

            return new ClassSymbolData(containingNamespace, classAccessibility, className, members);
        }

        private static List<MemberSymbolData> GetMemberSymbolData(IEnumerable<ISymbol> memberSymbols, Compilation compilation)
        {
            List<MemberSymbolData> result = [];

            foreach (var memberSymbol in memberSymbols)
            {
                var memberType = GetMemberType(memberSymbol);
                var (isSensitive, mask) = IsSensitive(memberSymbol);
                result.Add(new MemberSymbolData(
                    memberSymbol.Name,
                    IsDictionary(memberType, compilation),
                    IsEnumerable(memberType, compilation),
                    IsNullableType(memberSymbol),
                    isSensitive,
                    mask));
            }

            return result;
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

        private static string GetAccessibility(ISymbol typeSymbol)
        {
            return typeSymbol.DeclaredAccessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => "private"
            };
        }

        private static (bool, string?) IsSensitive(ISymbol symbol)
        {
            var sensitiveDataAttr = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == SensitiveDataAttributeName);

            if (sensitiveDataAttr is not null)
            {
                return (true, GetCustomRedactionValue(symbol));
            }

            return (false, null);
        }
        
        private static string? GetCustomRedactionValue(ISymbol member)
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

            return null;
        }

        private static IncrementalValueProvider<(ClassSymbolData? Type, string DefaultRedaction)> CombineProviders(
            IncrementalValuesProvider<ClassSymbolData?> typeDeclarations,
            IncrementalValueProvider<string> defaultRedactionConfig)
        {
            return typeDeclarations
                .Collect()
                .Select((nodes, _) => nodes.FirstOrDefault())
                .Combine(defaultRedactionConfig);
        }

        private static void Execute(SourceProductionContext context, string defaultRedactionValue, ClassSymbolData? classSymbolData)
        {
            if (!classSymbolData.HasValue) return;

            var sourceCode = GenerateToStringMethod(classSymbolData.Value, defaultRedactionValue);
            var fileName = $"{classSymbolData.Value.ClassName}.ToString.g.cs";

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(fileName, sourceCode);
        }

        private static string GenerateToStringMethod(ClassSymbolData classSymbolData, string defaultRedactionValue)
        {
            var sourceBuilder = new StringBuilder();
            AddUsingsAndNamespace(sourceBuilder, classSymbolData.ContainingNamespace);
            AddTypeDeclaration(sourceBuilder, classSymbolData.ClassAccessibility, classSymbolData.ClassName);
            AddToStringMethod(sourceBuilder, classSymbolData, defaultRedactionValue);
            return sourceBuilder.ToString();
        }

        private static void AddUsingsAndNamespace(StringBuilder sourceBuilder, string namespaceName)
        {
            sourceBuilder.AppendLine("using System;");
            sourceBuilder.AppendLine("using System.Text;");
            sourceBuilder.AppendLine("using System.Collections.Generic;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"namespace {namespaceName};");
            sourceBuilder.AppendLine();
        }

        private static void AddTypeDeclaration(StringBuilder sourceBuilder, string classAccessibility, string className)
        {
            sourceBuilder.AppendLine($"{classAccessibility} partial class {className}");
            sourceBuilder.AppendLine("{");
        }

        private static void AddToStringMethod(StringBuilder sourceBuilder, ClassSymbolData classSymbolData, string defaultRedactionValue)
        {
            sourceBuilder.AppendLine($"   public override string ToString()");
            sourceBuilder.AppendLine("    {");
            sourceBuilder.AppendLine("        var sb = new StringBuilder();");
            sourceBuilder.AppendLine($"        sb.Append(\"[{classSymbolData.ClassName}: \");");
            sourceBuilder.AppendLine();

            AppendMembers(sourceBuilder, classSymbolData.Members, defaultRedactionValue);

            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("        sb.Append(\"]\");");
            sourceBuilder.AppendLine("        return sb.ToString();");
            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");
        }

        private static IEnumerable<ISymbol> GetPublicMembers(ISymbol typeSymbol)
        {
            return ((INamespaceOrTypeSymbol)typeSymbol).GetMembers()
                .Where(m => m.Kind is SymbolKind.Property or SymbolKind.Field &&
                            m is { DeclaredAccessibility: Accessibility.Public, IsStatic: false });
        }

        private static void AppendMembers(StringBuilder sourceBuilder, IEnumerable<MemberSymbolData> members, string defaultRedactionValue)
        {
            var firstMember = true;
            foreach (var member in members)
            {
                var memberName = member.MemberName;
                var separator = firstMember ? "" : ", ";
                sourceBuilder.AppendLine($"        sb.Append(\"{separator}{memberName} = \");");

                if (member.IsSensitive)
                {
                    sourceBuilder.AppendLine($"        sb.Append(\"{member.Mask ?? defaultRedactionValue}\");");
                }
                else
                {
                    if (member.IsDictionary)
                    {
                        AppendDictionaryValue(sourceBuilder, memberName, member.IsNullableType);
                    }
                    else if (member.IsEnumerable)
                    {
                        AppendEnumerableValue(sourceBuilder, memberName, member.IsNullableType);
                    }
                    else
                    {
                        if (member.IsNullableType)
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
                
                firstMember = false;
            }
        }

        private static bool IsDictionary(ITypeSymbol type, Compilation compilation)
        {
            // Check for both generic and non-generic IEnumerable
            var dictionaryType = compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2");
            var dictionaryInterface = compilation.GetTypeByMetadataName("System.Collections.IDictionary");
            
            if (dictionaryType == null || dictionaryInterface == null) 
                return false;

            // Check if the type implements IEnumerable<T> or IEnumerable
            return type.AllInterfaces.Any(i => 
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, dictionaryType) ||
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, dictionaryInterface));
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

        private static bool IsNullableType(ISymbol type)
        {
            // Check if it's a nullable value type (e.g., int?)
            if (type is INamedTypeSymbol namedType && namedType.SpecialType == SpecialType.System_Nullable_T)
            {
                return true;
            }

            if (type is IFieldSymbol fieldSymbol)
            {
                return fieldSymbol.Type.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T or SpecialType.System_String;
            }

            if (type is ITypeSymbol typeSymbol)
            {
                // Check if it's a reference type with nullable annotation
                return typeSymbol.IsReferenceType && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
            }

            return false;
        }

        private static void AppendDictionaryValue(StringBuilder sourceBuilder, string memberName, bool isNullable)
        {
            if (isNullable)
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

        private static void AppendEnumerableValue(StringBuilder sourceBuilder, string memberName, bool isNullable)
        {
            if (isNullable)
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
    }
}
