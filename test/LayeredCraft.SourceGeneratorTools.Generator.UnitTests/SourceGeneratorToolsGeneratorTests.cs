using JetBrains.Annotations;

namespace LayeredCraft.SourceGeneratorTools.Generator.UnitTests;

[TestSubject(typeof(SourceGeneratorToolsGenerator))]
public class SourceGeneratorToolsGeneratorTests
{
    [Fact]
    public async Task Initialize_GeneratesOutput() => await GeneratorTestHelpers.Verify();

    [Fact]
    public async Task Initialize_SourceGeneratorToolsUsePublicModifier_GeneratesOutputWithPublic() =>
        await GeneratorTestHelpers.Verify(
            optionsProvider: new TestAnalyzerConfigOptionsProvider(
                new Dictionary<string, string>
                {
                    ["build_property.SourceGeneratorToolsUsePublicModifier"] = "true",
                }
            )
        );

    [Fact]
    public async Task Initialize_SourceGeneratorToolsIncludeSet_GeneratesOutput() =>
        await GeneratorTestHelpers.Verify(
            optionsProvider: new TestAnalyzerConfigOptionsProvider(
                new Dictionary<string, string>
                {
                    ["build_property.SourceGeneratorToolsInclude"] = "HashCode",
                }
            )
        );

    [Fact]
    public async Task Initialize_SourceGeneratorToolsExcludeSet_GeneratesOutput() =>
        await GeneratorTestHelpers.Verify(
            optionsProvider: new TestAnalyzerConfigOptionsProvider(
                new Dictionary<string, string>
                {
                    ["build_property.SourceGeneratorToolsExclude"] = "EquatableArray",
                }
            )
        );

    [Fact]
    public async Task Initialize_IncludeSupersedesExclude_GeneratesOutput() =>
        await GeneratorTestHelpers.Verify(
            optionsProvider: new TestAnalyzerConfigOptionsProvider(
                new Dictionary<string, string>
                {
                    ["build_property.SourceGeneratorToolsInclude"] = "HashCode",
                    ["build_property.SourceGeneratorToolsExclude"] = "EquatableArray",
                }
            )
        );
}
