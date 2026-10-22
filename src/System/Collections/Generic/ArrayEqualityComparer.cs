using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

/// <summary>
/// Provides a comparer to determine equality between two arrays of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the arrays.</typeparam>
public readonly struct ArrayEqualityComparer<T> : IEqualityComparer<T[]>
{
    /// <summary>
    /// Gets the default equality comparer for arrays of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This comparer performs element-by-element comparison using the default equality comparer
    /// for <typeparamref name="T"/>, or optimized memory comparison for blittable types.
    /// </remarks>
    public static ArrayEqualityComparer<T> Default { get; } = new();

    /// <summary>
    /// Determines whether the specified arrays are equal.
    /// </summary>
    /// <param name="x">The first array to compare.</param>
    /// <param name="y">The second array to compare.</param>
    /// <returns>true if the specified arrays are equal; otherwise, false.</returns>
    public bool Equals(T[]? x, T[]? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }
        if (x.Length != y.Length)
        {
            return false;
        }
        if (x.Length == 0)
        {
            return true;
        }
        var equalityComparer = EqualityComparer<T>.Default;

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return new ReadOnlySpan<T>(x).SequenceEqual(new ReadOnlySpan<T>(y), equalityComparer);
        }

        for (uint i = 0; i < x.Length; ++i)
        {
            if (!equalityComparer.Equals(x[i], y[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns a hash code for the specified array.
    /// </summary>
    /// <param name="arr">The array for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified array.</returns>
    public int GetHashCode(T[]? arr)
    {
        if (arr is null) return 0;
        var hash = new HashCode();
        for (uint i = 0; i < arr.Length; ++i)
        {
            hash.Add(arr[i]);
        }
        return hash.ToHashCode();
    }
}
