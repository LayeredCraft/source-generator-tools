using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LayeredCraft.SourceGeneratorTools.Generator.UnitTests;

internal static class GeneratorTestHelpers
{
    internal static Task Verify(
        string source = "",
        AnalyzerConfigOptionsProvider? optionsProvider = null
    )
    {
        var (driver, originalCompilation) = GenerateFromSource(
            source,
            optionsProvider: optionsProvider
        );

        driver.Should().NotBeNull();

        var result = driver.GetRunResult();

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

        return Verifier.Verify(driver).UseDirectory("Snapshots").DisableDiff();
    }

    private static (GeneratorDriver driver, Compilation compilation) GenerateFromSource(
        string source = "",
        Dictionary<string, ReportDiagnostic>? diagnosticsToSuppress = null,
        AnalyzerConfigOptionsProvider? optionsProvider = null
    )
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp14);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions, "InputFile.cs");

        // Add necessary references for generated code to compile
        // Get all referenced assemblies from the current assembly to ensure we have all required
        // types
        var referencedAssemblies = typeof(GeneratorTestHelpers)
            .Assembly.GetReferencedAssemblies()
            .Select(System.Reflection.Assembly.Load)
            .Where(a => a.GetName().Name?.StartsWith("System") == true)
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
            .. referencedAssemblies,
        ];

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
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

        if (optionsProvider is not null)
            driver = (CSharpGeneratorDriver)
                driver.WithUpdatedAnalyzerConfigOptions(optionsProvider);

        var updatedDriver = driver.RunGenerators(compilation, CancellationToken.None);

        return (updatedDriver, compilation);
    }
}
