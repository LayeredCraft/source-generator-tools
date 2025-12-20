using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LayeredCraft.SourceGeneratorTools.Generator.UnitTests;

internal static class GeneratorTestHelpers
{
    internal static Task Verify(string source, int expectedTrees = 1)
    {
        var (driver, originalCompilation) = GenerateFromSource(source);

        driver.Should().NotBeNull();

        var result = driver.GetRunResult();

        // result.Diagnostics.Length.Should().Be(0);s

        // Reparse generated trees with the same parse options as the original compilation
        // to ensure consistent syntax tree features (e.g., InterceptorsNamespaces)
        var parseOptions = originalCompilation.SyntaxTrees.First().Options;
        var reparsedTrees = result
            .GeneratedTrees.Select(tree =>
                CSharpSyntaxTree.ParseText(tree.GetText(), (CSharpParseOptions)parseOptions)
            )
            .ToArray();

        // Add generated trees to original compilation
        var outputCompilation = originalCompilation.AddSyntaxTrees(reparsedTrees);

        var errors = outputCompilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        errors
            .Should()
            .BeEmpty(
                "generated code should compile without errors, but found:\n"
                    + string.Join(
                        "\n",
                        errors.Select(e => $"  - {e.Id}: {e.GetMessage()} at {e.Location}")
                    )
            );

        result.GeneratedTrees.Length.Should().Be(expectedTrees);

        return Verifier.Verify(driver).UseDirectory("Snapshots").DisableDiff();
    }

    internal static (GeneratorDriver driver, Compilation compilation) GenerateFromSource(
        string source,
        Dictionary<string, ReportDiagnostic>? diagnosticsToSuppress = null
    )
    {
        IEnumerable<KeyValuePair<string, string>> features =
        [
            new("InterceptorsNamespaces", "MinimalLambda"),
        ];

        var parseOptions = CSharpParseOptions
            .Default.WithLanguageVersion(LanguageVersion.CSharp14)
            .WithFeatures(features);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions, "InputFile.cs");

        List<MetadataReference> references = [];

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.ConsoleApplication,
            nullableContextOptions: NullableContextOptions.Enable
        );

        if (diagnosticsToSuppress is not null)
            compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                diagnosticsToSuppress
            );

        var compilation = CSharpCompilation.Create(
            "Tests",
            [syntaxTree],
            references,
            compilationOptions
        );

        var generator = new SourceGeneratorToolsGenerator().AsSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        var updatedDriver = driver.RunGenerators(compilation, CancellationToken.None);

        return (updatedDriver, compilation);
    }
}
