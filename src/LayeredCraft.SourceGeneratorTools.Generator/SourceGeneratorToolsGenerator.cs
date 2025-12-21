using System;
using System.IO;
using System.Linq;
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

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get the MSBuild property (cached)
        var compilationOptions = context.AnalyzerConfigOptionsProvider.Select(
            static (provider, _) =>
            {
                provider.GlobalOptions.TryGetValue(
                    "build_property.SourceGeneratorToolsInclude",
                    out var include
                );
                provider.GlobalOptions.TryGetValue(
                    "build_property.SourceGeneratorToolsExclude",
                    out var exclude
                );
                provider.GlobalOptions.TryGetValue(
                    "build_property.SourceGeneratorToolsUsePublicModifier",
                    out var usePublic
                );

                return new CompilationOptions(include, exclude, bool.Parse(usePublic ?? "false"));
            }
        );

        // Only generate static content if condition is met
        context.RegisterSourceOutput(
            compilationOptions,
            static (ctx, compilationOptions) =>
            {
                // get inclusions
                var inclusions = compilationOptions.Include is not null
                    ? compilationOptions.Include!.Split(
                        [';'],
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    : [];

                // get exclusions
                var exclusions = compilationOptions.Exclude is not null
                    ? compilationOptions.Exclude!.Split(
                        [';'],
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    : [];

                // logic:
                // 1. if include is set, only those items are added and exclude is ignored.
                // 2. if exclude is set, all items is taken as base and excluded items are removed.
                // 3. default is all items
                string[]? featureKeys = null;
                if (inclusions.Length > 0)
                    featureKeys = inclusions.Distinct().ToArray();
                else if (exclusions.Length > 0)
                    featureKeys = GeneratorConstants.AllFeatures.Except(exclusions).ToArray();
                else
                    featureKeys = GeneratorConstants.AllFeatures;

                // get all district file references
                var filePaths = featureKeys
                    .Where(GeneratorConstants.Features.ContainsKey)
                    .SelectMany(key => GeneratorConstants.Features[key])
                    .Distinct()
                    .ToList();

                var assembly = typeof(SourceGeneratorToolsGenerator).Assembly;
                var assemblyNamespace = assembly.GetName().Name;

                // Load each file path specified in the feature
                foreach (var filePath in filePaths)
                {
                    // Convert file path to embedded resource name (add assembly namespace and
                    // replace / with .)
                    var resourceName = $"{assemblyNamespace}.{filePath.Replace('/', '.')}";

                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null)
                        continue;

                    var content = compilationOptions.UsePublicModifier
                        ? new StreamReader(stream).ReadToEnd()
                        : ConvertPublicToInternal(stream);

                    // Extract filename from the path
                    var fileName = Path.GetFileName(filePath);
                    var generatedFileName = Path.GetFileNameWithoutExtension(fileName) + ".g.cs";

                    var source = GeneratorConstants.GeneratedCodeHeader + content;

                    ctx.AddSource(generatedFileName, source);
                }
            }
        );
    }

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
