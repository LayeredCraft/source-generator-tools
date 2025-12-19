using System;
using System.Collections.Generic;
using System.Linq;

namespace LayeredCraft.SourceGeneratorTools.Types;

/// <summary>
/// Extension methods for creating <see cref="EquatableArray{T}"/> instances.
/// </summary>
public static class EquatableArrayExtensions
{
    /// <summary>
    /// Creates an <see cref="EquatableArray{T}"/> from the provided sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="enumerable">The source sequence.</param>
    /// <returns>An <see cref="EquatableArray{T}"/> containing the sequence elements.</returns>
    public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> enumerable)
        where T : IEquatable<T> => new(enumerable.ToArray());
}
