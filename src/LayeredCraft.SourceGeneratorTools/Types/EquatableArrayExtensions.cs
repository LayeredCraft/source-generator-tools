using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;

namespace System.Collections.Generic;

/// <summary>
/// Extension methods for creating <see cref="EquatableArray{T}"/> instances.
/// </summary>
public static class EquatableArrayExtensions
{
    extension<T>(IEnumerable<T> enumerable)
        where T : IEquatable<T>
    {
        /// <summary>
        /// Creates an <see cref="EquatableArray{T}"/> from the provided sequence.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="enumerable">The source sequence.</param>
        /// <returns>An <see cref="EquatableArray{T}"/> containing the sequence elements.</returns>
        public EquatableArray<T> ToEquatableArray() => new(enumerable.ToArray());
    }
}
