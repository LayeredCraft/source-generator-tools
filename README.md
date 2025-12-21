# SourceGeneratorTools

![Build Status](https://github.com/LayeredCraft/source-generator-tools/actions/workflows/build.yaml/badge.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET utility library providing reusable components for building C# Source Generators. Available as
both a runtime package and a compile-time source generator, offering type-safe data structures
specifically designed for source generator development.

## 📦 Packages

| Package                                         | NuGet                                                                                                                                                                   | Downloads                                                                                                                                                                    |
|-------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **LayeredCraft.SourceGeneratorTools**           | [![NuGet](https://img.shields.io/nuget/v/LayeredCraft.SourceGeneratorTools.svg)](https://www.nuget.org/packages/LayeredCraft.SourceGeneratorTools/)                     | [![Downloads](https://img.shields.io/nuget/dt/LayeredCraft.SourceGeneratorTools.svg)](https://www.nuget.org/packages/LayeredCraft.SourceGeneratorTools/)                     |
| **LayeredCraft.SourceGeneratorTools.Generator** | [![NuGet](https://img.shields.io/nuget/v/LayeredCraft.SourceGeneratorTools.Generator.svg)](https://www.nuget.org/packages/LayeredCraft.SourceGeneratorTools.Generator/) | [![Downloads](https://img.shields.io/nuget/dt/LayeredCraft.SourceGeneratorTools.Generator.svg)](https://www.nuget.org/packages/LayeredCraft.SourceGeneratorTools.Generator/) |

- **LayeredCraft.SourceGeneratorTools**: Runtime utilities library - use this when you need the
  types in your regular application
- **LayeredCraft.SourceGeneratorTools.Generator**: Source generator that embeds the utilities at
  compile-time - use this when building source generators or when you want zero runtime dependencies

## Installation

### Option 1: Using the Source Generator (Recommended)

Install via NuGet:

```bash
dotnet add package LayeredCraft.SourceGeneratorTools.Generator
```

The package reference in your `.csproj` should look like this:

```xml

<ItemGroup>
  <PackageReference Include="LayeredCraft.SourceGeneratorTools.Generator" Version="0.1.0-alpha"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false"/>
</ItemGroup>

<PropertyGroup>
<!-- Optional configuration -->
<SourceGeneratorToolsInclude>EquatableArray</SourceGeneratorToolsInclude>
<SourceGeneratorToolsUsePublicModifier>false</SourceGeneratorToolsUsePublicModifier>
</PropertyGroup>
```

> **Note:** The generator package automatically makes the `CompilerVisibleProperty` declarations
> available via its `.props` file, so you don't need to manually add `ItemGroup` entries for the
> properties.

Generates utility code at compile-time without adding runtime dependencies. The generated code
becomes part of your project with no additional DLLs to ship.

### Option 2: Using the Runtime Package

Install via NuGet:

```bash
dotnet add package LayeredCraft.SourceGeneratorTools
```

The package reference in your `.csproj`:

```xml

<ItemGroup>
  <PackageReference Include="LayeredCraft.SourceGeneratorTools" Version="0.1.0-alpha"/>
</ItemGroup>
```

Adds the utilities as a traditional NuGet package dependency. The DLL is included in your build
output and distributed with your application.

## When to Use Which Approach

### Quick Comparison

| Aspect                    | Source Generator               | Runtime Package         |
|---------------------------|--------------------------------|-------------------------|
| **Deployment**            | No runtime DLL                 | Ships with your app     |
| **Dependencies**          | Zero at runtime                | Package dependency      |
| **Primary Use Case**      | Building source generators     | Using utilities in apps |
| **Accessibility Control** | Configurable (public/internal) | Fixed (as published)    |
| **Build Time**            | Slightly longer (generation)   | Standard                |

### Detailed Guidance

**Use the Source Generator (`LayeredCraft.SourceGeneratorTools.Generator`) when:**

- **You're building your own source generator** - This is the primary use case. When you create a
  source generator that uses `EquatableArray<T>` or other utilities, those types need to be
  available at compile-time in the generator assembly
- **You want zero runtime dependencies** - The generated code becomes part of your project's source,
  so there's no additional DLL to ship with your application
- **You need to bundle third-party packages** - As explained
  in [this article on using 3rd-party libraries with Roslyn source generators](https://www.thinktecture.com/en/net/roslyn-source-generators-using-3rd-party-libraries/),
  when your source generator depends on external packages, those dependencies must be bundled with
  the generator. Source generation eliminates this complexity by embedding the code directly.
- **You want control over accessibility** - Configure whether generated types are `public` or
  `internal` via `SourceGeneratorToolsUsePublicModifier`

**Use the Runtime Package (`LayeredCraft.SourceGeneratorTools`) when:**

- **You just need the utility types in your regular application code** - If you're using
  `EquatableArray<T>` or `HashCode` in a non-source-generator project
- **You prefer traditional package dependency management** - Some teams prefer explicit runtime
  dependencies for clarity in dependency graphs
- **You're not building a source generator** - For regular applications that just need the utilities

## Configuration Reference

When using the source generator package, you can configure its behavior using MSBuild properties.

### MSBuild Properties

| Property                                | Type   | Default     | Description                                                                          |
|-----------------------------------------|--------|-------------|--------------------------------------------------------------------------------------|
| `SourceGeneratorToolsInclude`           | string | `""` (all)  | Semicolon-separated list of features to include. Empty means all features.           |
| `SourceGeneratorToolsExclude`           | string | `""` (none) | Semicolon-separated list of features to exclude. Ignored when Include is set.        |
| `SourceGeneratorToolsUsePublicModifier` | string | `"false"`   | Set to `"true"` to generate types with `public` accessibility instead of `internal`. |

> **Note:** The boolean property accepts string values `"true"` or `"false"` (case-insensitive).

### Available Features

- **`EquatableArray`** - Generates the complete suite:
  - `EquatableArray<T>` struct
  - `EquatableArrayExtensions` class
  - `HashCode` utility class

- **`HashCode`** - Generates only:
  - `HashCode` utility class (polyfill for `System.HashCode.Combine` on .NET Standard 2.0)

### Configuration Examples

**Example 1: Include only HashCode polyfill with public accessibility**

```xml

<PropertyGroup>
  <SourceGeneratorToolsInclude>HashCode</SourceGeneratorToolsInclude>
  <SourceGeneratorToolsUsePublicModifier>true</SourceGeneratorToolsUsePublicModifier>
</PropertyGroup>
```

**Example 2: Include all features but exclude HashCode (generates only EquatableArray and
extensions)**

```xml

<PropertyGroup>
  <SourceGeneratorToolsExclude>HashCode</SourceGeneratorToolsExclude>
</PropertyGroup>
```

**Example 3: Default behavior (all features, internal accessibility)**

```xml
<!-- No configuration needed - just reference the package -->
<ItemGroup>
  <PackageReference Include="LayeredCraft.SourceGeneratorTools.Generator" Version="0.1.0-alpha"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false"/>
</ItemGroup>
```

## Features

<details>
<summary><strong>EquatableArray&lt;T&gt;</strong> - Value-based array equality with a read-only wrapper</summary>

### Overview

A value-based array wrapper that provides value equality semantics, making it ideal for use in
source generators and caching scenarios.

### Key Features

- **Value equality semantics** - Arrays compared by content, not reference
- **Read-only by convention** - Safe to share across threads when the backing array is not mutated
- **Zero-allocation access** via `AsSpan()` for high-performance scenarios
- **Index support** - Access elements via an indexer or slice via `AsSpan()`
- **Thread-safe for read operations** - No locking required
- **Generic `IEnumerable<T>` support** - Works with LINQ and foreach

### Usage Example

```csharp
using LayeredCraft.SourceGeneratorTools.Types;

var array1 = new EquatableArray<string>(new[] { "foo", "bar" });
var array2 = new EquatableArray<string>(new[] { "foo", "bar" });

// Value equality - useful for caching in source generators
Console.WriteLine(array1.Equals(array2)); // True
Console.WriteLine(array1.GetHashCode() == array2.GetHashCode()); // True
```

> **Note:** When using the source generator package, the generated code keeps the original
> namespaces (`LayeredCraft.SourceGeneratorTools.Types`,
`LayeredCraft.SourceGeneratorTools.Utilities`, and `System.Collections.Generic`) and uses `internal`
> accessibility by default (configurable via `SourceGeneratorToolsUsePublicModifier`). When using the
> runtime package, the types are `public`.

</details>

<details>
<summary><strong>HashCode</strong> - Polyfill for System.HashCode.Combine on .NET Standard 2.0</summary>

### Overview

A utility class that provides hash code combination functionality for .NET Standard 2.0+
environments, offering the same API as the built-in `System.HashCode.Combine` methods.

### Key Features

- **Polyfill for older frameworks** - Brings `System.HashCode.Combine` to .NET Standard 2.0+
- **Multiple overloads** - Combine 1-8 values in a single call
- **Randomized hashing** - Per-process seed, consistent within a run
- **Zero allocations** - Efficient stack-based implementation

### Usage Example

```csharp
using LayeredCraft.SourceGeneratorTools.Utilities;

// Combine multiple values into a single hash code
int hash = HashCode.Combine(value1, value2, value3);

// Use in GetHashCode implementations
public override int GetHashCode() => HashCode.Combine(Name, Age, Email);
```

</details>

## Documentation

- **GitHub Repository**: https://github.com/LayeredCraft/source-generator-tools
- **Unit Tests**: Comprehensive examples available in `test/LayeredCraft.SourceGeneratorTools.UnitTests`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
