using System.Runtime.CompilerServices;

namespace LayeredCraft.SourceGeneratorTools.Generator.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
