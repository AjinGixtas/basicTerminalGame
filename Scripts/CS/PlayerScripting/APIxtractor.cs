using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;
using Godot;
using System.Text.RegularExpressions;

class APIxtractor {
    public record MethodData(string Name, List<(string Type, string Name)> Parameters, string ReturnType, string XmlDoc);
    public record ClassData(string Name, List<MethodData> Methods, string XmlDoc);

    public static ClassData ParseClass(string csCode) {
        var tree = CSharpSyntaxTree.ParseText(csCode);
        var root = tree.GetRoot();
        var classNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        string classXmlDoc = GetXmlDoc(classNode);

        var methods = classNode.Members.OfType<MethodDeclarationSyntax>().Select(m =>
            new MethodData(
                m.Identifier.ValueText,
                m.ParameterList.Parameters.Select(p => (p.Type?.ToString() ?? "object", p.Identifier.ValueText)).ToList(),
                m.ReturnType.ToString(),
                GetXmlDoc(m)
            )
        ).ToList();

        return new ClassData(classNode.Identifier.ValueText, methods, classXmlDoc);
    }

    // Extract XML documentation comments from syntax node
    private static string GetXmlDoc(MemberDeclarationSyntax node) {
        var trivia = node.GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        if (trivia == null) return "";

        var xmlString = string.Join("\n", trivia.Content.Select(c => c.ToFullString()));

        // Wrap in root to parse easily
        try {
            var xml = XElement.Parse($"<root>{xmlString}</root>");
            return xml.ToString();
        } catch {
            return "";
        }
    }

    // Parse XML doc and convert to Lua comment lines
    private static string FormatXmlDocToLuaComments(string xmlDoc, MethodData method = null) {
        if (string.IsNullOrWhiteSpace(xmlDoc))
            return "";

        var sb = new StringBuilder();

        try {
            var root = XElement.Parse(xmlDoc);

            // Summary tag
            var summary = root.Element("summary");
            if (summary != null) {
                var summaryText = CleanXmlText(summary.Value);
                sb.AppendLine($"--- {summaryText}");
            }

            // Param tags
            foreach (var param in root.Elements("param")) {
                var nameAttr = param.Attribute("name");
                if (nameAttr != null) {
                    var paramName = nameAttr.Value;
                    var paramText = CleanXmlText(param.Value);
                    // Only add if param exists in method signature
                    if (method == null || method.Parameters.Any(p => p.Name == paramName))
                        sb.AppendLine($"---@param {paramName} any -- {paramText}");
                }
            }

            // Return tag
            var returns = root.Element("returns");
            if (returns != null) {
                var returnText = CleanXmlText(returns.Value);
                sb.AppendLine($"---@return any -- {returnText}");
            }
        } catch (Exception e) {
            // Parsing failed, skip XML doc
            GD.PrintErr(e.Message);
            GD.PrintErr(e.StackTrace);
        }

        return sb.ToString();
    }

    // Helper to clean up XML text content (trim, remove extra spaces/newlines)
    private static string CleanXmlText(string text) {
        GD.Print(text);
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++) {
            lines[i] = lines[i].TrimStart('/', ' ').TrimEnd(' ');
        }
        return string.Join("\n---", lines);
    }

    public static string GenerateLuaStub(ClassData cls) {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(cls.XmlDoc)) {
            sb.AppendLine(FormatXmlDocToLuaComments(cls.XmlDoc));
        }
        sb.AppendLine($"---@class {cls.Name}");
        sb.AppendLine($"local {cls.Name} = {{}}");
        sb.AppendLine();

        foreach (var m in cls.Methods) {
            sb.AppendLine(FormatXmlDocToLuaComments(m.XmlDoc, m));
            // Add type params/return (if no XML doc, fallback to simple mapping)
            if (string.IsNullOrWhiteSpace(m.XmlDoc)) {
                foreach (var p in m.Parameters)
                    sb.AppendLine($"---@param {p.Name} {LuaTypeMapper.TranslateCSharpTypeToLua(p.Type)}");
                sb.AppendLine($"---@return {LuaTypeMapper.TranslateCSharpTypeToLua(m.ReturnType)}");
            }
            sb.AppendLine($"function {cls.Name}.{m.Name}({string.Join(", ", m.Parameters.Select(p => p.Name))}) end");
            sb.AppendLine();
        }

        sb.AppendLine($"return {cls.Name}");
        return sb.ToString();
    }

    public static string MapCSharpTypeToLua(string csharpType) {
        return csharpType switch {
            "int" => "number",
            "float" => "number",
            "double" => "number",
            "bool" => "boolean",
            "string" => "string",
            "void" => "nil",

            _ => "any"
        };
    }

    public static string GenerateKeywordDict(ClassData cls) {
        var sb = new StringBuilder();
        sb.AppendLine("var syntaxKeywords = new Dictionary<CodeType, List<string>>");
        sb.AppendLine("{");
        sb.AppendLine("    [CodeType.Class] = new List<string> {");
        sb.AppendLine($"        \"{cls.Name}\"");
        sb.AppendLine("    },");
        sb.AppendLine("    [CodeType.Function] = new List<string>");
        sb.AppendLine("    {");
        foreach (var m in cls.Methods)
            sb.AppendLine($"        \"{m.Name}\",");
        sb.AppendLine("    }");
        sb.AppendLine("};");
        return sb.ToString();
    }
}

public static class LuaTypeMapper {
    private static readonly Dictionary<string, string> BaseTypeMap = new() {
        ["int"] = "number",
        ["float"] = "number",
        ["double"] = "number",
        ["bool"] = "boolean",
        ["string"] = "string",
        ["void"] = "nil"
    };

    public static string TranslateCSharpTypeToLua(string csharpType) {
        csharpType = csharpType.Trim();

        // Array: e.g., "string[]"
        if (csharpType.EndsWith("[]")) {
            var elementType = csharpType[..^2];
            return $"{TranslateCSharpTypeToLua(elementType)}[]";
        }

        // Generic types: List<>, Dictionary<>, Tuple<>
        if (Regex.IsMatch(csharpType, @"^\w+<.*>$")) {
            var match = Regex.Match(csharpType, @"^(\w+)<(.+)>$");
            if (!match.Success) return "any";

            var outer = match.Groups[1].Value;
            var inner = match.Groups[2].Value;
            var args = SplitGenericArgs(inner);

            switch (outer) {
                case "List":
                    return $"{TranslateCSharpTypeToLua(args[0])}[]";

                case "Dictionary":
                    return $"table<{TranslateCSharpTypeToLua(args[0])}, {TranslateCSharpTypeToLua(args[1])}>";

                case "Tuple":
                    var translated = args.ConvertAll(TranslateCSharpTypeToLua);
                    return $"table<{string.Join(", ", translated)}>";

                default:
                    return "any"; // unknown generic
            }
        }

        // Base types
        if (BaseTypeMap.TryGetValue(csharpType, out var luaType))
            return luaType;

        return "any"; // fallback
    }

    private static List<string> SplitGenericArgs(string input) {
        var args = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < input.Length; i++) {
            if (input[i] == '<') depth++;
            else if (input[i] == '>') depth--;
            else if (input[i] == ',' && depth == 0) {
                args.Add(input.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }

        args.Add(input.Substring(start).Trim());
        return args;
    }
}