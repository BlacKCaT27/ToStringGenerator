using System.Text;

namespace Bcss.ToStringGenerator;

public static class ToStringGeneratorHelper
{
    public static string GenerateToStringMethod(ClassSymbolData classSymbolData, string defaultRedactionValue)
    {
        var sourceBuilder = new StringBuilder();
        AddUsingsAndNamespace(sourceBuilder, classSymbolData.ContainingNamespace);
        AddTypeDeclaration(sourceBuilder, classSymbolData.ClassAccessibility, classSymbolData.ClassName);
        AddToStringMethod(sourceBuilder, classSymbolData, defaultRedactionValue);
        return sourceBuilder.ToString();
    }

    public static void AddUsingsAndNamespace(StringBuilder sourceBuilder, string namespaceName)
    {
        sourceBuilder.AppendLine("using System;");
        sourceBuilder.AppendLine("using System.Text;");
        sourceBuilder.AppendLine("using System.Collections.Generic;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine($"namespace {namespaceName};");
        sourceBuilder.AppendLine();
    }

    public static void AddTypeDeclaration(StringBuilder sourceBuilder, string classAccessibility, string className)
    {
        sourceBuilder.AppendLine($"{classAccessibility} partial class {className}");
        sourceBuilder.AppendLine("{");
    }

    public static void AddToStringMethod(StringBuilder sourceBuilder, ClassSymbolData classSymbolData, string defaultRedactionValue)
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
    
    public static void AppendMembers(StringBuilder sourceBuilder, IEnumerable<MemberSymbolData> members, string defaultRedactionValue)
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
    
    public static void AppendDictionaryValue(StringBuilder sourceBuilder, string memberName, bool isNullable)
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

    public static void AppendDictionaryContents(StringBuilder sourceBuilder, string memberName)
    {
        sourceBuilder.AppendLine("            sb.Append('[');");
        sourceBuilder.AppendLine($"            var {memberName}Enumerator = {memberName}.GetEnumerator();");
        sourceBuilder.AppendLine($"            if ({memberName}Enumerator.MoveNext())");
        sourceBuilder.AppendLine("            {");
        sourceBuilder.AppendLine($"                var pair = {memberName}Enumerator.Current;");
        sourceBuilder.AppendLine("                sb.Append('{');");
        sourceBuilder.AppendLine("                sb.Append(pair.Key.ToString());");
        sourceBuilder.AppendLine("                sb.Append(\", \");");
        sourceBuilder.AppendLine("                sb.Append(pair.Value.ToString());");
        sourceBuilder.AppendLine("                sb.Append('}');");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine($"                while ({memberName}Enumerator.MoveNext())");
        sourceBuilder.AppendLine("                {");
        sourceBuilder.AppendLine("                    sb.Append(\", \");");
        sourceBuilder.AppendLine($"                    pair = {memberName}Enumerator.Current;");
        sourceBuilder.AppendLine("                    sb.Append('{');");
        sourceBuilder.AppendLine("                    sb.Append(pair.Key.ToString());");
        sourceBuilder.AppendLine("                    sb.Append(\", \");");
        sourceBuilder.AppendLine("                    sb.Append(pair.Value.ToString());");
        sourceBuilder.AppendLine("                    sb.Append('}');");
        sourceBuilder.AppendLine("                }");
        sourceBuilder.AppendLine("            }");
        sourceBuilder.AppendLine("            sb.Append(']');");
    }

    public static void AppendEnumerableValue(StringBuilder sourceBuilder, string memberName, bool isNullable)
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

    public static void AppendEnumerableContents(StringBuilder sourceBuilder, string memberName)
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