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
}
