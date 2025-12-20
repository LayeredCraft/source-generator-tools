using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;

namespace LayeredCraft.SourceGeneratorTools.Generator;

[Generator]
public class SourceGeneratorToolsGenerator : IIncrementalGenerator
{
    private const string PublicModifier = "public";
    private const string InternalModifier = "internal";
    private const string NoReplaceComment = "// no-replace";
    private const int PublicModifierLength = 6;

    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterPostInitializationOutput(ctx =>
        {
            // For now, hardcode to EquatableArray - in the future this could be configurable
            var featureKeys = new[] { "EquatableArray" };

            var assembly = typeof(SourceGeneratorToolsGenerator).Assembly;

            foreach (var featureKey in featureKeys)
            {
                if (!GeneratorConstants.Features.TryGetValue(featureKey, out var feature))
                    continue;

                // Get the assembly namespace for resource lookup
                var assemblyNamespace = assembly.GetName().Name;

                // Load each file path specified in the feature
                foreach (var filePath in feature.FolderPaths)
                {
                    // Convert file path to embedded resource name (add assembly namespace and
                    // replace / with .)
                    var resourceName = $"{assemblyNamespace}.{filePath.Replace('/', '.')}";

                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null)
                        continue;

                    var content = ConvertPublicToInternal(stream);

                    // Extract filename from the path
                    var fileName = Path.GetFileName(filePath);
                    var generatedFileName = Path.GetFileNameWithoutExtension(fileName) + ".g.cs";

                    var source = GeneratorConstants.GeneratedCodeHeader + content;

                    ctx.AddSource(generatedFileName, source);
                }
            }
        });

    /// <summary>
    ///     Sets any line that starts with "public" to be "internal". This only impacts top-level
    ///     statements. This behavior can be skipped by adding a "// no-replace" at the end of the line.
    /// </summary>
    private static string ConvertPublicToInternal(Stream stream)
    {
        var sb = new StringBuilder((int)stream.Length);
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line)
            if (
                line.StartsWith(PublicModifier)
                && !line.Contains(NoReplaceComment, StringComparison.Ordinal)
            )
                sb.Append(InternalModifier).AppendLine(line[PublicModifierLength..]);
            else
                sb.AppendLine(line);

        return sb.ToString();
    }
}
