using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Collections.Generic;

/// <summary>
/// Provides a comparer to determine equality between two arrays of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the arrays.</typeparam>
public readonly struct ArrayEqualityComparer<T> : IEqualityComparer<T[]>
{
    private readonly IEqualityComparer<T>? _elementComparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayEqualityComparer{T}"/> structure
    /// using the specified element comparer.
    /// </summary>
    /// <param name="elementComparer">The comparer used to compare elements and calculate their hash codes.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="elementComparer"/> is <see langword="null"/>.
    /// </exception>
    public ArrayEqualityComparer(IEqualityComparer<T> elementComparer)
    {
        ArgumentNullException.ThrowIfNull(elementComparer);
        _elementComparer = elementComparer;
    }

    /// <summary>
    /// Gets an array equality comparer that uses <see cref="EqualityComparer{T}.Default"/>
    /// to compare elements and calculate their hash codes.
    /// </summary>
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

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return new ReadOnlySpan<T>(x).SequenceEqual(new ReadOnlySpan<T>(y), _elementComparer);
        }

        var equalityComparer = _elementComparer ?? EqualityComparer<T>.Default;

        ref T xRef = ref x[0];
        ref T yRef = ref y[0];

        for (uint i = 0; i < x.Length; ++i)
        {
            if (!equalityComparer.Equals(Unsafe.Add(ref xRef, i), Unsafe.Add(ref yRef, i)))
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

        var equalityComparer = _elementComparer ?? EqualityComparer<T>.Default;

        var hash = new HashCode();
#if NET5_0_OR_GREATER
        ref T itemRef = ref MemoryMarshal.GetArrayDataReference(arr);
#endif
        for (uint i = 0; i < arr.Length; ++i)
        {
#if NET5_0_OR_GREATER
            hash.Add(Unsafe.Add(ref itemRef, i), equalityComparer);
#else
            hash.Add(arr[i], equalityComparer);
#endif
        }

        return hash.ToHashCode();
    }
}