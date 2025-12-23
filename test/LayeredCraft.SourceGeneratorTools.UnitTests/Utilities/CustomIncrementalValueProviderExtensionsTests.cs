using System.Collections.Immutable;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LayeredCraft.SourceGeneratorTools.UnitTests.Utilities;

[TestSubject(typeof(CustomIncrementalValueProviderExtensions))]
public class CustomIncrementalValueProviderExtensionsTests
{
    #region Helper Methods

    private static CSharpCompilation CreateCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("");
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        };

        return CSharpCompilation.Create(
            "TestCompilation",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    #endregion

    #region WhereNotNull Tests for Value Types

    [Fact]
    public void WhereNotNull_WithNullableIntProvider_FiltersOutNulls()
    {
        // Arrange
        var compilation = CreateCompilation();
        var generator = new TestIncrementalGenerator<int>([1, null, 2, null, 3]);

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out _,
                out _,
                TestContext.Current.CancellationToken
            );

        // Assert
        var result = driver.GetRunResult();
        result.Results.Should().HaveCount(1);
        generator.CapturedValues.Should().HaveCount(3);
        generator.CapturedValues.Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void WhereNotNull_WithAllNullNullableIntProvider_ReturnsEmpty()
    {
        // Arrange
        var compilation = CreateCompilation();
        var generator = new TestIncrementalGenerator<int>([null, null, null]);

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out _,
                out _,
                TestContext.Current.CancellationToken
            );

        // Assert
        var result = driver.GetRunResult();
        result.Results.Should().HaveCount(1);
        generator.CapturedValues.Should().BeEmpty();
    }

    [Fact]
    public void WhereNotNull_WithAllNonNullNullableIntProvider_ReturnsAllValues()
    {
        // Arrange
        var compilation = CreateCompilation();
        var generator = new TestIncrementalGenerator<int>([1, 2, 3, 4, 5]);

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out _,
                out _,
                TestContext.Current.CancellationToken
            );

        // Assert
        var result = driver.GetRunResult();
        result.Results.Should().HaveCount(1);
        generator.CapturedValues.Should().HaveCount(5);
        generator.CapturedValues.Should().ContainInOrder(1, 2, 3, 4, 5);
    }

    [Fact]
    public void WhereNotNull_WithEmptyNullableIntProvider_ReturnsEmpty()
    {
        // Arrange
        var compilation = CreateCompilation();
        var generator = new TestIncrementalGenerator<int>([]);

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out _,
                out _,
                TestContext.Current.CancellationToken
            );

        // Assert
        var result = driver.GetRunResult();
        result.Results.Should().HaveCount(1);
        generator.CapturedValues.Should().BeEmpty();
    }

    #endregion

    #region WhereNotNull Tests for Reference Types

    [Fact]
    public void WhereNotNull_WithNullableStringProvider_FiltersOutNulls()
    {
        // Arrange
        var compilation = CreateCompilation();
        var generator = new TestIncrementalGeneratorForReferenceType<string>([
            "foo",
            null,
            "bar",
            null,
            "baz",
        ]);

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out _,
                out _,
                TestContext.Current.CancellationToken
            );

        // Assert
        var result = driver.GetRunResult();
        result.Results.Should().HaveCount(1);
        generator.CapturedValues.Should().HaveCount(3);
        generator.CapturedValues.Should().ContainInOrder("foo", "bar", "baz");
    }

    [Fact]
    public void WhereNotNull_WithAllNullStringProvider_ReturnsEmpty()
    {
        // Arrange
        var compilation = CreateCompilation();
        var generator = new TestIncrementalGeneratorForReferenceType<string>([null, null, null]);

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out _,
                out _,
                TestContext.Current.CancellationToken
            );

        // Assert
        var result = driver.GetRunResult();
        result.Results.Should().HaveCount(1);
        generator.CapturedValues.Should().BeEmpty();
    }

    [Fact]
    public void WhereNotNull_WithAllNonNullStringProvider_ReturnsAllValues()
    {
        // Arrange
        var compilation = CreateCompilation();
        var generator = new TestIncrementalGeneratorForReferenceType<string>([
            "a",
            "b",
            "c",
            "d",
            "e",
        ]);

        // Act
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out _,
                out _,
                TestContext.Current.CancellationToken
            );

        // Assert
        var result = driver.GetRunResult();
        result.Results.Should().HaveCount(1);
        generator.CapturedValues.Should().HaveCount(5);
        generator.CapturedValues.Should().ContainInOrder("a", "b", "c", "d", "e");
    }

    #endregion

    #region Test Generators

    private class TestIncrementalGenerator<T>(IEnumerable<T?> values) : IIncrementalGenerator
        where T : struct
    {
        public List<T> CapturedValues { get; } = [];

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.CompilationProvider.SelectMany(
                (_, _) => values.ToImmutableArray()
            );

            var filtered = provider.WhereNotNull();

            context.RegisterSourceOutput(
                filtered,
                (_, value) =>
                {
                    CapturedValues.Add(value);
                }
            );
        }
    }

    private class TestIncrementalGeneratorForReferenceType<T>(IEnumerable<T?> values)
        : IIncrementalGenerator
        where T : class
    {
        public List<T> CapturedValues { get; } = [];

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.CompilationProvider.SelectMany(
                (_, _) => values.ToImmutableArray()
            );

            var filtered = provider.WhereNotNull();

            context.RegisterSourceOutput(
                filtered,
                (_, value) =>
                {
                    CapturedValues.Add(value);
                }
            );
        }
    }

    #endregion
}
