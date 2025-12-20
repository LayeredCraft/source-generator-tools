using System.IO;
using Microsoft.CodeAnalysis;

namespace LayeredCraft.SourceGeneratorTools.Generator;

[Generator]
public class SourceGeneratorToolsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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

                    using var reader = new StreamReader(stream);
                    var content = reader.ReadToEnd();

                    // Extract filename from the path
                    var fileName = Path.GetFileName(filePath);
                    var generatedFileName = Path.GetFileNameWithoutExtension(fileName) + ".g.cs";

                    var source = GeneratorConstants.GeneratedCodeHeader + content;

                    ctx.AddSource(generatedFileName, source);
                }
            }
        });

        // Process additional files marked for source generation
        var filesToGenerate = context
            .AdditionalTextsProvider.Combine(context.AnalyzerConfigOptionsProvider)
            .Where(pair =>
            {
                var (file, options) = pair;
                options
                    .GetOptions(file)
                    .TryGetValue(
                        "build_metadata.AdditionalFiles.SourceGenerate",
                        out var shouldGenerate
                    );
                return shouldGenerate == "true";
            })
            .Select(
                (pair, cancellationToken) =>
                {
                    var (file, options) = pair;
                    var content = file.GetText(cancellationToken)?.ToString() ?? string.Empty;
                    var fileName = Path.GetFileName(file.Path);

                    // Get optional output path override
                    options
                        .GetOptions(file)
                        .TryGetValue(
                            "build_metadata.AdditionalFiles.OutputPath",
                            out var outputPath
                        );

                    return (FileName: fileName, Content: content, OutputPath: outputPath);
                }
            );

        context.RegisterSourceOutput(
            filesToGenerate,
            (ctx, file) =>
            {
                // Use OutputPath if specified, otherwise use the original filename with .g.cs
                // extension
                var generatedFileName = !string.IsNullOrEmpty(file.OutputPath)
                    ? file.OutputPath
                    : Path.GetFileNameWithoutExtension(file.FileName) + ".g.cs";

                // Ensure we have a valid filename
                if (string.IsNullOrWhiteSpace(generatedFileName))
                    return;

                var source = GeneratorConstants.GeneratedCodeHeader + file.Content;

                ctx.AddSource(generatedFileName!, source);
            }
        );
    }
}
