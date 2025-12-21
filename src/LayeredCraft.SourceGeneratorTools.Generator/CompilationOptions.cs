namespace LayeredCraft.SourceGeneratorTools.Generator;

internal readonly record struct CompilationOptions(
    string? Include,
    string? Exclude,
    string? UsePublicModifier
);
