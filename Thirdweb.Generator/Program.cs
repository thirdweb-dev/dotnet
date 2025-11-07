using System.Globalization;
using System.Text;
using System.Xml.Linq;
using NJsonSchema;
using NSwag;
using NSwag.CodeGeneration.CSharp;

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "thirdweb.sln")))
        {
            return dir.FullName;
        }
        dir = dir.Parent;
    }
    // Fallback to current directory
    return Directory.GetCurrentDirectory();
}

static void GenerateLlmsTxt(string repoRoot)
{
    var xmlPath = Path.Combine(repoRoot, "Thirdweb", "bin", "Release", "netstandard2.1", "Thirdweb.xml");
    var outputPath = Path.Combine(repoRoot, "llms.txt");

    if (!File.Exists(xmlPath))
    {
        Console.WriteLine($"XML documentation not found at {xmlPath}");
        Console.WriteLine("Please build the project in Release mode first.");
        Environment.Exit(1);
    }

    Console.WriteLine($"Reading XML documentation from {xmlPath}...");
    var doc = XDocument.Load(xmlPath);
    var sb = new StringBuilder();

    _ = sb.AppendLine("THIRDWEB .NET SDK - API DOCUMENTATION");
    _ = sb.AppendLine("=====================================");
    _ = sb.AppendLine();

    var assembly = doc.Root?.Element("assembly")?.Element("name")?.Value;
    if (assembly != null)
    {
        _ = sb.AppendLine($"Assembly: {assembly}");
        _ = sb.AppendLine();
    }

    var members = doc.Root?.Element("members")?.Elements("member");
    if (members != null)
    {
        foreach (var member in members)
        {
            var name = member.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            _ = sb.AppendLine(new string('-', 80));
            _ = sb.AppendLine(name);
            _ = sb.AppendLine(new string('-', 80));

            // Parse signature for better display
            var signature = ParseMemberSignature(name);
            if (signature != null)
            {
                _ = sb.AppendLine();
                _ = sb.AppendLine($"KIND: {signature.Kind}");
                if (!string.IsNullOrEmpty(signature.ReturnType))
                {
                    _ = sb.AppendLine($"RETURN TYPE: {signature.ReturnType}");
                }
            }

            // Group param elements by name attribute
            var paramDocs = member.Elements("param").Select(p => new { Name = p.Attribute("name")?.Value, Description = p.Value.Trim() }).Where(p => !string.IsNullOrEmpty(p.Name)).ToList();

            // Display parameters with their names and types
            if (signature?.Parameters != null && signature.Parameters.Count > 0)
            {
                _ = sb.AppendLine();
                _ = sb.AppendLine("PARAMETERS:");
                for (var i = 0; i < signature.Parameters.Count; i++)
                {
                    var param = signature.Parameters[i];
                    // Try to get the actual parameter name from documentation
                    var paramDoc = i < paramDocs.Count ? paramDocs[i] : null;
                    var paramName = paramDoc?.Name ?? param.Name;

                    _ = sb.AppendLine($"  - {paramName} ({param.Type})");
                    if (paramDoc != null && !string.IsNullOrEmpty(paramDoc.Description))
                    {
                        _ = sb.AppendLine($"    {NormalizeXmlText(paramDoc.Description).Replace("\n", "\n    ")}");
                    }
                }
            }

            // Display other elements (summary, remarks, returns, exception, etc.)
            foreach (var element in member.Elements())
            {
                if (element.Name.LocalName == "param")
                {
                    continue; // Already handled above
                }

                var content = element.Value.Trim();
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }

                _ = sb.AppendLine();
                var elementName = element.Name.LocalName.ToUpper();

                // Add attribute info for exceptions and type params
                var nameAttr = element.Attribute("name")?.Value;
                var crefAttr = element.Attribute("cref")?.Value;
                if (!string.IsNullOrEmpty(nameAttr))
                {
                    elementName += $" ({nameAttr})";
                }
                else if (!string.IsNullOrEmpty(crefAttr))
                {
                    elementName += $" ({crefAttr})";
                }

                _ = sb.AppendLine($"{elementName}:");
                _ = sb.AppendLine(NormalizeXmlText(content));
            }

            _ = sb.AppendLine();
        }
    }

    File.WriteAllText(outputPath, sb.ToString());
    Console.WriteLine($"Generated llms.txt at {outputPath}");
}

static MemberSignature? ParseMemberSignature(string memberName)
{
    if (string.IsNullOrEmpty(memberName) || memberName.Length < 2)
    {
        return null;
    }

    var kind = memberName[0] switch
    {
        'M' => "Method",
        'P' => "Property",
        'T' => "Type",
        'F' => "Field",
        'E' => "Event",
        _ => "Unknown",
    };

    var fullSignature = memberName[2..]; // Remove "M:", "P:", etc.

    // For methods, parse parameters and return type
    if (memberName[0] == 'M')
    {
        var parameters = new List<ParameterInfo>();
        var methodName = fullSignature;
        var returnType = "System.Threading.Tasks.Task"; // Default for async methods

        // Extract parameters from signature
        var paramStart = fullSignature.IndexOf('(');
        if (paramStart >= 0)
        {
            methodName = fullSignature[..paramStart];
            var paramEnd = fullSignature.LastIndexOf(')');
            if (paramEnd > paramStart)
            {
                var paramString = fullSignature[(paramStart + 1)..paramEnd];
                if (!string.IsNullOrEmpty(paramString))
                {
                    var paramParts = SplitParameters(paramString);
                    for (var i = 0; i < paramParts.Count; i++)
                    {
                        var paramType = SimplifyTypeName(paramParts[i]);
                        parameters.Add(new ParameterInfo { Name = $"param{i + 1}", Type = paramType });
                    }
                }
            }

            // Check if there's a return type after the parameters
            if (paramEnd + 1 < fullSignature.Length && fullSignature[paramEnd + 1] == '~')
            {
                returnType = SimplifyTypeName(fullSignature[(paramEnd + 2)..]);
            }
        }

        return new MemberSignature
        {
            Kind = kind,
            Parameters = parameters,
            ReturnType = parameters.Count > 0 || fullSignature.Contains("Async") ? returnType : null,
        };
    }

    return new MemberSignature { Kind = kind };
}

static List<string> SplitParameters(string paramString)
{
    var parameters = new List<string>();
    var current = new StringBuilder();
    var depth = 0;

    foreach (var c in paramString)
    {
        if (c is '{' or '<')
        {
            depth++;
        }
        else if (c is '}' or '>')
        {
            depth--;
        }
        else if (c == ',' && depth == 0)
        {
            parameters.Add(current.ToString());
            _ = current.Clear();
            continue;
        }

        _ = current.Append(c);
    }

    if (current.Length > 0)
    {
        parameters.Add(current.ToString());
    }

    return parameters;
}

static string SimplifyTypeName(string fullTypeName)
{
    // Remove assembly information
    var typeName = fullTypeName.Split(',')[0];

    // Simplify common generic types
    typeName = typeName
        .Replace("System.Threading.Tasks.Task{", "Task<")
        .Replace("System.Collections.Generic.List{", "List<")
        .Replace("System.Collections.Generic.Dictionary{", "Dictionary<")
        .Replace("System.Nullable{", "Nullable<")
        .Replace("System.String", "string")
        .Replace("System.Int32", "int")
        .Replace("System.Int64", "long")
        .Replace("System.Boolean", "bool")
        .Replace("System.Byte", "byte")
        .Replace("System.Object", "object")
        .Replace('{', '<')
        .Replace('}', '>');

    return typeName;
}

static string NormalizeXmlText(string text)
{
    var lines = text.Split('\n');
    var normalized = new StringBuilder();

    foreach (var line in lines)
    {
        var trimmed = line.Trim();
        if (!string.IsNullOrEmpty(trimmed))
        {
            _ = normalized.AppendLine(trimmed);
        }
    }

    return normalized.ToString().TrimEnd();
}

var cmdArgs = Environment.GetCommandLineArgs();
if (cmdArgs.Length > 1 && cmdArgs[1] == "--llms")
{
    var root = FindRepoRoot();
    GenerateLlmsTxt(root);
    return;
}

var specUrl = "https://api.thirdweb.com/openapi.json";
var repoRoot = FindRepoRoot();
var outputPath = Path.Combine(repoRoot, "Thirdweb", "Thirdweb.Api", "ThirdwebApi.cs");

Console.WriteLine($"Loading OpenAPI from {specUrl}...");
var document = await OpenApiDocument.FromUrlAsync(specUrl);

Console.WriteLine("Deduplicating enum values across all schemas (inline + components)...");

var visited = new HashSet<object>();

void DedupeEnumOnSchema(JsonSchema schema, string? debugPath)
{
    if (schema == null)
    {
        return;
    }
    if (!visited.Add(schema))
    {
        return;
    }

    var s = schema.ActualSchema ?? schema;

    if (s.Enumeration != null && s.Enumeration.Count > 0)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var newEnum = new List<object>();
        var enumList = s.Enumeration.ToList();

        for (var i = 0; i < enumList.Count; i++)
        {
            var v = enumList[i];
            var key = Convert.ToString(v, CultureInfo.InvariantCulture) ?? "<null>";
            if (seen.Add(key))
            {
                newEnum.Add(v!);
            }
        }

        if (newEnum.Count != s.Enumeration.Count)
        {
            s.Enumeration.Clear();
            foreach (var v in newEnum)
            {
                s.Enumeration.Add(v);
            }

            // Remove x-enumNames to avoid misalignment with deduped values
            if (s.ExtensionData != null && s.ExtensionData.ContainsKey("x-enumNames"))
            {
                _ = s.ExtensionData.Remove("x-enumNames");
            }

            Console.WriteLine($"  - Deduped enum at {debugPath ?? "<unknown>"}: {newEnum.Count} unique values");
        }
    }

    // Recurse into nested schemas
    foreach (var p in s.Properties)
    {
        DedupeEnumOnSchema(p.Value, $"{debugPath}/properties/{p.Key}");
    }

    if (s.AdditionalPropertiesSchema != null)
    {
        DedupeEnumOnSchema(s.AdditionalPropertiesSchema, $"{debugPath}/additionalProperties");
    }

    if (s.Item != null)
    {
        DedupeEnumOnSchema(s.Item, $"{debugPath}/items");
    }

    foreach (var a in s.AllOf)
    {
        DedupeEnumOnSchema(a, $"{debugPath}/allOf");
    }
    foreach (var o in s.OneOf)
    {
        DedupeEnumOnSchema(o, $"{debugPath}/oneOf");
    }
    foreach (var a in s.AnyOf)
    {
        DedupeEnumOnSchema(a, $"{debugPath}/anyOf");
    }
    if (s.Not != null)
    {
        DedupeEnumOnSchema(s.Not, $"{debugPath}/not");
    }
}

// Components
foreach (var kvp in document.Components.Schemas)
{
    DedupeEnumOnSchema(kvp.Value, $"#/components/schemas/{kvp.Key}");
}

// Parameters
foreach (var path in document.Paths)
{
    foreach (var opKvp in path.Value)
    {
        var op = opKvp.Value;
        foreach (var prm in op.Parameters)
        {
            if (prm.Schema != null)
            {
                DedupeEnumOnSchema(prm.Schema, $"#/paths{path.Key}/{op.OperationId}/parameters/{prm.Name}");
            }
        }
        // Request body
        var rb = op.RequestBody;
        if (rb?.Content != null)
        {
            foreach (var c in rb.Content)
            {
                if (c.Value.Schema != null)
                {
                    DedupeEnumOnSchema(c.Value.Schema, $"#/paths{path.Key}/{op.OperationId}/requestBody/{c.Key}");
                }
            }
        }
        // Responses
        foreach (var resp in op.Responses)
        {
            if (resp.Value.Content != null)
            {
                foreach (var c in resp.Value.Content)
                {
                    if (c.Value.Schema != null)
                    {
                        DedupeEnumOnSchema(c.Value.Schema, $"#/paths{path.Key}/{op.OperationId}/responses/{resp.Key}/{c.Key}");
                    }
                }
            }
        }
    }
}

var settings = new CSharpClientGeneratorSettings
{
    ClassName = "ThirdwebApiClient",
    CSharpGeneratorSettings = { Namespace = "Thirdweb.Api", ClassStyle = NJsonSchema.CodeGeneration.CSharp.CSharpClassStyle.Poco },
    GenerateClientClasses = true,
    GenerateDtoTypes = true,
    GenerateExceptionClasses = true,
    ClientClassAccessModifier = "public",
    // Use our HttpClient wrapper type
    HttpClientType = "ThirdwebHttpClientWrapper",
};

Console.WriteLine("Generating C# client code...");
var generator = new CSharpClientGenerator(document, settings);
var code = generator.GenerateFile();

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllTextAsync(outputPath, code);
Console.WriteLine($"Wrote generated client to {outputPath}");

internal class MemberSignature
{
    public string Kind { get; set; } = "";
    public List<ParameterInfo>? Parameters { get; set; }
    public string? ReturnType { get; set; }
}

internal class ParameterInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}
