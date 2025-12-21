using System.Runtime.CompilerServices;

namespace LayeredCraft.SourceGeneratorTools.Generator.UnitTests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize() => VerifySourceGenerators.Initialize();
}
