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

Install via NuGet:

```bash
dotnet add package LayeredCraft.SourceGeneratorTools
```

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
