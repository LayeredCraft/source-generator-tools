using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LayeredCraft.SourceGeneratorTools.Generator.UnitTests;

// Custom implementation to provide MSBuild properties
internal sealed class TestAnalyzerConfigOptionsProvider(Dictionary<string, string> options)
    : AnalyzerConfigOptionsProvider
{
    private readonly TestAnalyzerConfigOptions _globalOptions = new(options);

    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
        TestAnalyzerConfigOptions.Empty;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
        TestAnalyzerConfigOptions.Empty;
}

internal sealed class TestAnalyzerConfigOptions(Dictionary<string, string> options)
    : AnalyzerConfigOptions
{
    public static readonly TestAnalyzerConfigOptions Empty = new(new Dictionary<string, string>());

    public override bool TryGetValue(string key, out string value) =>
        options.TryGetValue(key, out value!);
}
