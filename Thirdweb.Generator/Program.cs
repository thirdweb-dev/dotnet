using System.Globalization;
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
