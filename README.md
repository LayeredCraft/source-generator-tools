# SourceGeneratorTools

![Build Status](https://github.com/LayeredCraft/source-generator-tools/actions/workflows/build.yaml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/LayeredCraft.SourceGeneratorTools.svg)](https://www.nuget.org/packages/LayeredCraft.SourceGeneratorTools/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET utility library providing reusable components for building C# Source Generators. This library offers type-safe, immutable data structures specifically designed for source generator development.

## Key Highlights

- **Zero-reflection overhead** - Compile-time utilities designed for source generators
- **Type-safe immutable collections** with value equality semantics
- **Immutable by design** - Thread-safe for read operations
- **Polyfill support** for .NET Standard 2.0+ compatibility
- **Comprehensive unit test coverage** with 150+ test cases

## Installation

### For General Use

If you're using this library in regular application code:

```bash
dotnet add package LayeredCraft.SourceGeneratorTools
```

### For Source Generator Projects

When building a source generator that uses these utilities, additional configuration is required to properly package the library with your analyzer. This ensures the library is available at compile-time and doesn't create transitive dependencies.

Add to your source generator's `.csproj` file:

**1. Package reference with special attributes:**

```xml
<ItemGroup>
  <PackageReference
    Include="LayeredCraft.SourceGeneratorTools"
    PrivateAssets="all"
    GeneratePathProperty="true"
  />
</ItemGroup>
```

**2. Required project properties:**

```xml
<PropertyGroup>
  <TargetFramework>netstandard2.0</TargetFramework>
  <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
  <IsRoslynComponent>true</IsRoslynComponent>
  <IncludeBuildOutput>false</IncludeBuildOutput>
  <GetTargetPathDependsOn>$(GetTargetPathDependsOn);GetDependencyTargetPaths</GetTargetPathDependsOn>
</PropertyGroup>
```

**3. Include the library in your analyzer package:**

```xml
<ItemGroup>
  <None
    Include="$(PkgLayeredCraft_SourceGeneratorTools)\lib\netstandard2.0\LayeredCraft.SourceGeneratorTools.dll"
    Pack="true"
    PackagePath="analyzers/dotnet/cs"
    Visible="false"
  />
</ItemGroup>
```

**4. Add MSBuild target for dependency paths:**

```xml
<Target Name="GetDependencyTargetPaths">
  <ItemGroup>
    <TargetPathWithTargetPlatformMoniker
      Include="$(PkgLayeredCraft_SourceGeneratorTools)\lib\netstandard2.0\LayeredCraft.SourceGeneratorTools.dll"
      IncludeRuntimeDependency="false"
    />
  </ItemGroup>
</Target>
```

> **Why these steps?** The `PrivateAssets="all"` prevents the library from flowing as a transitive dependency. The `GeneratePathProperty="true"` creates the `$(PkgLayeredCraft_SourceGeneratorTools)` variable. The `<None>` element packages the DLL with your analyzer, and the custom target makes it available at compile-time.

## Quick Start

### EquatableArray&lt;T&gt; - Value-based Array Equality

```csharp
using LayeredCraft.SourceGeneratorTools.Types;

var array1 = new EquatableArray<string>(new[] { "foo", "bar" });
var array2 = new EquatableArray<string>(new[] { "foo", "bar" });

// Value equality - useful for caching in source generators
Console.WriteLine(array1.Equals(array2)); // True
Console.WriteLine(array1.GetHashCode() == array2.GetHashCode()); // True
```

## Features

### EquatableArray&lt;T&gt;

- **Value equality semantics** - Arrays compared by content, not reference
- **Immutable by design** - Safe to share across threads
- **Zero-allocation access** via `AsSpan()` for high-performance scenarios
- **Index and Range support** - Modern C# slicing syntax
- **Thread-safe for read operations** - No locking required
- **Generic `IEnumerable<T>` support** - Works with LINQ and foreach

## Documentation

- **GitHub Repository**: https://github.com/LayeredCraft/source-generator-tools
- **Unit Tests**: Comprehensive examples available in `test/LayeredCraft.SourceGeneratorTools.UnitTests`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
