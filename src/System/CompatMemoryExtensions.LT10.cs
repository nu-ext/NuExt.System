// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.
#if !NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

partial class CompatMemoryExtensions
{
    /// <summary>
    /// Searches for the specified value and returns true if found. If not found, returns false.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null) =>
        span.IndexOf(value, comparer) >= 0;

    /// <summary>
    /// Searches for any occurrence of the specified <paramref name="value0"/> or <paramref name="value1"/>, and returns true if found. If not found, returns false.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">One of the values to search for.</param>
    /// <param name="value1">One of the values to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null) =>
        span.IndexOfAny(value0, value1, comparer) >= 0;

    /// <summary>
    /// Searches for any occurrence of the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, and returns true if found. If not found, returns false.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">One of the values to search for.</param>
    /// <param name="value1">One of the values to search for.</param>
    /// <param name="value2">One of the values to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null) =>
        span.IndexOfAny(value0, value1, value2, comparer) >= 0;

    /// <summary>
    /// Searches for any occurrence of any of the specified <paramref name="values"/> and returns true if found. If not found, returns false.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The set of values to search for.</param>
    /// <param name="comparer">The comparer to use. If <see langword="null"/>, <see cref="EqualityComparer{T}.Default"/> is used.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values, IEqualityComparer<T>? comparer = null) =>
        span.IndexOfAny(values, comparer) >= 0;

    /// <summary>
    /// Searches for any value other than the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value">A value to avoid.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// True if any value other than <paramref name="value"/> is present in the span.
    /// If all of the values are <paramref name="value"/>, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null) =>
        span.IndexOfAnyExcept(value, comparer) >= 0;

    /// <summary>
    /// Searches for any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// True if any value other than <paramref name="value0"/> and <paramref name="value1"/> is present in the span.
    /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null) =>
        span.IndexOfAnyExcept(value0, value1, comparer) >= 0;

    /// <summary>
    /// Searches for any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid.</param>
    /// <param name="value2">A value to avoid.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// True if any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/> is present in the span.
    /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null) =>
        span.IndexOfAnyExcept(value0, value1, value2, comparer) >= 0;

    /// <summary>
    /// Searches for any value other than the specified <paramref name="values"/>.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The values to avoid.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// True if any value other than those in <paramref name="values"/> is present in the span.
    /// If all of the values are in <paramref name="values"/>, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values, IEqualityComparer<T>? comparer = null) =>
        span.IndexOfAnyExcept(values, comparer) >= 0;

    /// <summary>Searches for the first index of any value other than the specified <paramref name="value"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">A value to avoid.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value"/>.
    /// If all of the values are <paramref name="value"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value),
                        span.Length);
                }
                else if (size == sizeof(int))
                {
                    return SpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, int>(value),
                        span.Length);
                }
                else if (size == sizeof(long))
                {
                    return SpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, long>(value),
                        span.Length);
                }
            }

            return IndexOfAnyExceptDefaultComparer(span, value);
            static int IndexOfAnyExceptDefaultComparer(ReadOnlySpan<T> span, T value)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    if (!EqualityComparer<T>.Default.Equals(span[i], value))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return IndexOfAnyExceptComparer(span, value, comparer);
            static int IndexOfAnyExceptComparer(ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                for (int i = 0; i < span.Length; i++)
                {
                    if (!comparer.Equals(span[i], value))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
    /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value0),
                        Unsafe.Reinterpret<T, byte>(value1),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value0),
                        Unsafe.Reinterpret<T, short>(value1),
                        span.Length);
                }
            }

            return IndexOfAnyExceptDefaultComparer(span, value0, value1);
            static int IndexOfAnyExceptDefaultComparer(ReadOnlySpan<T> span, T value0, T value1)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    if (!EqualityComparer<T>.Default.Equals(span[i], value0) &&
                        !EqualityComparer<T>.Default.Equals(span[i], value1))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return IndexOfAnyExceptComparer(span, value0, value1, comparer);
            static int IndexOfAnyExceptComparer(ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                for (int i = 0; i < span.Length; i++)
                {
                    if (!comparer.Equals(span[i], value0) &&
                        !comparer.Equals(span[i], value1))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <param name="value2">A value to avoid</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
    /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value0),
                        Unsafe.Reinterpret<T, byte>(value1),
                        Unsafe.Reinterpret<T, byte>(value2),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.IndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value0),
                        Unsafe.Reinterpret<T, short>(value1),
                        Unsafe.Reinterpret<T, short>(value2),
                        span.Length);
                }
            }

            return IndexOfAnyExceptDefaultComparer(span, value0, value1, value2);
            static int IndexOfAnyExceptDefaultComparer(ReadOnlySpan<T> span, T value0, T value1, T value2)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    if (!EqualityComparer<T>.Default.Equals(span[i], value0) &&
                        !EqualityComparer<T>.Default.Equals(span[i], value1) &&
                        !EqualityComparer<T>.Default.Equals(span[i], value2))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return IndexOfAnyExceptComparer(span, value0, value1, value2, comparer);
            static int IndexOfAnyExceptComparer(ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;
                for (int i = 0; i < span.Length; i++)
                {
                    if (!comparer.Equals(span[i], value0) &&
                        !comparer.Equals(span[i], value1) &&
                        !comparer.Equals(span[i], value2))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>Searches for the first index of any value other than the specified <paramref name="values"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The values to avoid.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
    /// If all of the values are in <paramref name="values"/>, returns -1.
    /// </returns>
    public static int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values, IEqualityComparer<T>? comparer = null)
    {
        switch (values.Length)
        {
            case 0:
                return span.IsEmpty ? -1 : 0;

            case 1:
                return IndexOfAnyExcept(span, values[0], comparer);

            case 2:
                return IndexOfAnyExcept(span, values[0], values[1], comparer);

            case 3:
                return IndexOfAnyExcept(span, values[0], values[1], values[2], comparer);

            default:
                for (int i = 0; i < span.Length; i++)
                {
                    if (!values.Contains(span[i], comparer))
                    {
                        return i;
                    }
                }

                return -1;
        }
    }

    /// <summary>Searches for the last index of any value other than the specified <paramref name="value"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">A value to avoid.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value"/>.
    /// If all of the values are <paramref name="value"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value),
                        span.Length);
                }
                else if (size == sizeof(int))
                {
                    return SpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, int>(value),
                        span.Length);
                }
                else if (size == sizeof(long))
                {
                    return SpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, long>(value),
                        span.Length);
                }
            }

            return LastIndexOfAnyExceptDefaultComparer(span, value);
            static int LastIndexOfAnyExceptDefaultComparer(ReadOnlySpan<T> span, T value)
            {
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (!EqualityComparer<T>.Default.Equals(span[i], value))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return LastIndexOfAnyExceptComparer(span, value, comparer);
            static int LastIndexOfAnyExceptComparer(ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (!comparer.Equals(span[i], value))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
    /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value0),
                        Unsafe.Reinterpret<T, byte>(value1),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value0),
                        Unsafe.Reinterpret<T, short>(value1),
                        span.Length);
                }
            }

            return LastIndexOfAnyExceptDefaultComparer(span, value0, value1);
            static int LastIndexOfAnyExceptDefaultComparer(ReadOnlySpan<T> span, T value0, T value1)
            {
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (!EqualityComparer<T>.Default.Equals(span[i], value0) &&
                        !EqualityComparer<T>.Default.Equals(span[i], value1))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return LastIndexOfAnyExceptComparer(span, value0, value1, comparer);
            static int LastIndexOfAnyExceptComparer(ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (!comparer.Equals(span[i], value0) &&
                        !comparer.Equals(span[i], value1))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <param name="value2">A value to avoid</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
    /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value0),
                        Unsafe.Reinterpret<T, byte>(value1),
                        Unsafe.Reinterpret<T, byte>(value2),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.LastIndexOfAnyExceptValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value0),
                        Unsafe.Reinterpret<T, short>(value1),
                        Unsafe.Reinterpret<T, short>(value2),
                        span.Length);
                }
            }

            return LastIndexOfAnyExceptDefaultComparer(span, value0, value1, value2);
            static int LastIndexOfAnyExceptDefaultComparer(ReadOnlySpan<T> span, T value0, T value1, T value2)
            {
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (!EqualityComparer<T>.Default.Equals(span[i], value0) &&
                        !EqualityComparer<T>.Default.Equals(span[i], value1) &&
                        !EqualityComparer<T>.Default.Equals(span[i], value2))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return LastIndexOfAnyExceptComparer(span, value0, value1, value2, comparer);
            static int LastIndexOfAnyExceptComparer(ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (!comparer.Equals(span[i], value0) &&
                        !comparer.Equals(span[i], value1) &&
                        !comparer.Equals(span[i], value2))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>Searches for the last index of any value other than the specified <paramref name="values"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The values to avoid.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than those in <paramref name="values"/>.
    /// If all of the values are in <paramref name="values"/>, returns -1.
    /// </returns>
    public static int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values, IEqualityComparer<T>? comparer = null)
    {
        switch (values.Length)
        {
            case 0:
                return span.Length - 1;

            case 1:
                return LastIndexOfAnyExcept(span, values[0], comparer);

            case 2:
                return LastIndexOfAnyExcept(span, values[0], values[1], comparer);

            case 3:
                return LastIndexOfAnyExcept(span, values[0], values[1], values[2], comparer);

            default:
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (!values.Contains(span[i], comparer))
                    {
                        return i;
                    }
                }

                return -1;
        }
    }

    /// <summary>
    /// Searches for the specified value and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOf<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                    return SpanHelpers.IndexOfValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value),
                        span.Length);

                if (size == sizeof(short))
                    return SpanHelpers.IndexOfValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value),
                        span.Length);

                if (size == sizeof(int))
                    return SpanHelpers.IndexOfValueType(
                        ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, int>(value),
                        span.Length);

                if (size == sizeof(long))
                    return SpanHelpers.IndexOfValueType(
                        ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, long>(value),
                        span.Length);
            }

            return IndexOfDefaultComparer(span, value);
            static int IndexOfDefaultComparer(ReadOnlySpan<T> span, T value)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(span[i], value))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return IndexOfComparer(span, value, comparer);
            static int IndexOfComparer(ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;
                for (int i = 0; i < span.Length; i++)
                {
                    if (comparer.Equals(span[i], value))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>
    /// Searches for the specified sequence and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The sequence to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, IEqualityComparer<T>? comparer = null)
    {
        if (RuntimeHelpers.IsKnownBitwiseEquatable<T>() && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            int size = Unsafe.SizeOf<T>();
            if (size == sizeof(byte))
                return SpanHelpers.IndexOf(
                    ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                    span.Length,
                    ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
                    value.Length);

            if (size == sizeof(char))
                return SpanHelpers.IndexOf(
                    ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)),
                    span.Length,
                    ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(value)),
                    value.Length);
        }

        return IndexOfComparer(span, value, comparer);
        static int IndexOfComparer(ReadOnlySpan<T> span, ReadOnlySpan<T> value, IEqualityComparer<T>? comparer)
        {
            if (value.Length == 0)
            {
                return 0;
            }

            comparer ??= EqualityComparer<T>.Default;

            int total = 0;
            while (!span.IsEmpty)
            {
                int pos = span.IndexOf(value[0], comparer);
                if (pos < 0)
                {
                    break;
                }

                if (span.Slice(pos + 1).StartsWith(value.Slice(1), comparer))
                {
                    return total + pos;
                }

                total += pos + 1;
                span = span.Slice(pos + 1);
            }

            return -1;
        }
    }

    /// <summary>
    /// Searches for the specified value and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOf<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.LastIndexOfValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.LastIndexOfValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value),
                        span.Length);
                }
                else if (size == sizeof(int))
                {
                    return SpanHelpers.LastIndexOfValueType(
                        ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, int>(value),
                        span.Length);
                }
                else if (size == sizeof(long))
                {
                    return SpanHelpers.LastIndexOfValueType(
                        ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, long>(value),
                        span.Length);
                }
            }

            return LastIndexOfDefaultComparer(span, value);
            static int LastIndexOfDefaultComparer(ReadOnlySpan<T> span, T value)
            {
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (EqualityComparer<T>.Default.Equals(span[i], value))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return LastIndexOfComparer(span, value, comparer);
            static int LastIndexOfComparer(ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (comparer.Equals(span[i], value))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>
    /// Searches for the specified sequence and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The sequence to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, IEqualityComparer<T>? comparer = null)
    {
        if (RuntimeHelpers.IsKnownBitwiseEquatable<T>() && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            int size = Unsafe.SizeOf<T>();
            if (size == sizeof(byte))
            {
                return SpanHelpers.LastIndexOf(
                    ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                    span.Length,
                    ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
                    value.Length);
            }
            if (size == sizeof(char))
            {
                return SpanHelpers.LastIndexOf(
                    ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)),
                    span.Length,
                    ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(value)),
                    value.Length);
            }
        }

        return LastIndexOfComparer(span, value, comparer);
        static int LastIndexOfComparer(ReadOnlySpan<T> span, ReadOnlySpan<T> value, IEqualityComparer<T>? comparer)
        {
            if (value.Length == 0)
            {
                return span.Length;
            }

            comparer ??= EqualityComparer<T>.Default;

            int pos = span.Length;
            while (true)
            {
                pos = span.Slice(0, pos).LastIndexOf(value[0], comparer);
                if (pos < 0)
                {
                    break;
                }

                if (span.Slice(pos + 1).StartsWith(value.Slice(1), comparer))
                {
                    return pos;
                }
            }

            return -1;
        }
    }

    /// <summary>
    /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">One of the values to search for.</param>
    /// <param name="value1">One of the values to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.IndexOfAnyValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value0),
                        Unsafe.Reinterpret<T, byte>(value1),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.IndexOfAnyValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value0),
                        Unsafe.Reinterpret<T, short>(value1),
                        span.Length);
                }
            }

            return IndexOfAnyDefaultComparer(span, value0, value1);
            static int IndexOfAnyDefaultComparer(ReadOnlySpan<T> span, T value0, T value1)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(span[i], value0) ||
                        EqualityComparer<T>.Default.Equals(span[i], value1))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return IndexOfAnyComparer(span, value0, value1, comparer);
            static int IndexOfAnyComparer(ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;
                for (int i = 0; i < span.Length; i++)
                {
                    if (comparer.Equals(span[i], value0) ||
                        comparer.Equals(span[i], value1))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>
    /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">One of the values to search for.</param>
    /// <param name="value1">One of the values to search for.</param>
    /// <param name="value2">One of the values to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.IndexOfAnyValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value0),
                        Unsafe.Reinterpret<T, byte>(value1),
                        Unsafe.Reinterpret<T, byte>(value2),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.IndexOfAnyValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value0),
                        Unsafe.Reinterpret<T, short>(value1),
                        Unsafe.Reinterpret<T, short>(value2),
                        span.Length);
                }
            }

            return IndexOfAnyDefaultComparer(span, value0, value1, value2);
            static int IndexOfAnyDefaultComparer(ReadOnlySpan<T> span, T value0, T value1, T value2)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(span[i], value0) ||
                        EqualityComparer<T>.Default.Equals(span[i], value1) ||
                        EqualityComparer<T>.Default.Equals(span[i], value2))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return IndexOfAnyComparer(span, value0, value1, value2, comparer);
            static int IndexOfAnyComparer(ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;
                for (int i = 0; i < span.Length; i++)
                {
                    if (comparer.Equals(span[i], value0) ||
                        comparer.Equals(span[i], value1) ||
                        comparer.Equals(span[i], value2))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>
    /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The set of values to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    public static int IndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values, IEqualityComparer<T>? comparer = null)
    {
        switch (values.Length)
        {
            case 0:
                return -1;

            case 1:
                return IndexOf(span, values[0], comparer);

            case 2:
                return IndexOfAny(span, values[0], values[1], comparer);

            case 3:
                return IndexOfAny(span, values[0], values[1], values[2], comparer);

            default:
                comparer ??= EqualityComparer<T>.Default;

                for (int i = 0; i < span.Length; i++)
                {
                    if (values.Contains(span[i], comparer))
                    {
                        return i;
                    }
                }

                return -1;
        }
    }

    /// <summary>
    /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">One of the values to search for.</param>
    /// <param name="value1">One of the values to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.LastIndexOfAnyValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value0),
                        Unsafe.Reinterpret<T, byte>(value1),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.LastIndexOfAnyValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value0),
                        Unsafe.Reinterpret<T, short>(value1),
                        span.Length);
                }
            }

            return LastIndexOfAnyDefaultComparer(span, value0, value1);
            static int LastIndexOfAnyDefaultComparer(ReadOnlySpan<T> span, T value0, T value1)
            {
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (EqualityComparer<T>.Default.Equals(span[i], value0) ||
                        EqualityComparer<T>.Default.Equals(span[i], value1))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return LastIndexOfAnyComparer(span, value0, value1, comparer);
            static int LastIndexOfAnyComparer(ReadOnlySpan<T> span, T value0, T value1, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (comparer.Equals(span[i], value0) ||
                        comparer.Equals(span[i], value1))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>
    /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">One of the values to search for.</param>
    /// <param name="value1">One of the values to search for.</param>
    /// <param name="value2">One of the values to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.LastIndexOfAnyValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value0),
                        Unsafe.Reinterpret<T, byte>(value1),
                        Unsafe.Reinterpret<T, byte>(value2),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.LastIndexOfAnyValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value0),
                        Unsafe.Reinterpret<T, short>(value1),
                        Unsafe.Reinterpret<T, short>(value2),
                        span.Length);
                }
            }

            return LastIndexOfAnyDefaultComparer(span, value0, value1, value2);
            static int LastIndexOfAnyDefaultComparer(ReadOnlySpan<T> span, T value0, T value1, T value2)
            {
                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (EqualityComparer<T>.Default.Equals(span[i], value0) ||
                        EqualityComparer<T>.Default.Equals(span[i], value1) ||
                        EqualityComparer<T>.Default.Equals(span[i], value2))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        else
        {
            return LastIndexOfAnyComparer(span, value0, value1, value2, comparer);
            static int LastIndexOfAnyComparer(ReadOnlySpan<T> span, T value0, T value1, T value2, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (comparer.Equals(span[i], value0) ||
                        comparer.Equals(span[i], value1) ||
                        comparer.Equals(span[i], value2))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }

    /// <summary>
    /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The set of values to search for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values, IEqualityComparer<T>? comparer = null)
    {
        switch (values.Length)
        {
            case 0:
                return -1;

            case 1:
                return LastIndexOf(span, values[0], comparer);

            case 2:
                return LastIndexOfAny(span, values[0], values[1], comparer);

            case 3:
                return LastIndexOfAny(span, values[0], values[1], values[2], comparer);

            default:
                comparer ??= EqualityComparer<T>.Default;

                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (values.Contains(span[i], comparer))
                    {
                        return i;
                    }
                }

                return -1;
        }
    }

    /// <summary>
    /// Determines the relative order of the sequences being compared by comparing the elements using IComparable{T}.CompareTo(T).
    /// </summary>
    public static int SequenceCompareTo<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other, IComparer<T>? comparer = null)
    {
        int minLength = Math.Min(span.Length, other.Length);
        comparer ??= Comparer<T>.Default;

        for (int i = 0; i < minLength; i++)
        {
            int c = comparer.Compare(span[i], other[i]);
            if (c != 0)
            {
                return c;
            }
        }

        return span.Length.CompareTo(other.Length);
    }

    /// <summary>
    /// Determines whether the beginning of this span matches the specified sequence.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The sequence to compare to the start of <paramref name="span"/>.</param>
    /// <param name="comparer">An optional comparer to use for element equality checks.</param>
    /// <typeparam name="T">The type of elements in the span and value.</typeparam>
    /// <returns><see langword="true"/> if <paramref name="span"/> starts with <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, IEqualityComparer<T>? comparer = null) =>
        value.Length <= span.Length &&
        span.Slice(0, value.Length).SequenceEqual(value, comparer);

    /// <summary>
    /// Determines whether the end of this span matches the specified sequence.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The sequence to compare to the end of <paramref name="span"/>.</param>
    /// <param name="comparer">An optional comparer to use for element equality checks.</param>
    /// <typeparam name="T">The type of elements in the span and value.</typeparam>
    /// <returns><see langword="true"/> if <paramref name="span"/> ends with <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, IEqualityComparer<T>? comparer = null) =>
        value.Length <= span.Length &&
        span.Slice(span.Length - value.Length).SequenceEqual(value, comparer);

    /// <summary>
    /// Determines whether the specified value appears at the start of the span.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value to compare.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns><see langword="true" /> if <paramref name="value" /> matches the beginning of <paramref name="span" />; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWith<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null) =>
        span.Length != 0 &&
        (comparer is null ? EqualityComparer<T>.Default.Equals(span[0], value) : comparer.Equals(span[0], value));

    /// <summary>
    /// Determines whether the specified value appears at the end of the span.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value to compare.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns><see langword="true" /> if <paramref name="value" /> matches the end of <paramref name="span" />; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWith<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null) =>
        span.Length != 0 &&
#if NETFRAMEWORK || NETSTANDARD2_0
        (comparer is null ? EqualityComparer<T>.Default.Equals(span[span.Length - 1], value) : comparer.Equals(span[span.Length - 1], value));
#else
        (comparer is null ? EqualityComparer<T>.Default.Equals(span[^1], value) : comparer.Equals(span[^1], value));
#endif

    /// <summary>
    /// Replaces all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span in which the elements should be replaced.</param>
    /// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
    /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Replace<T>(this Span<T> span, T oldValue, T newValue, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    ref byte src = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span));
                    SpanHelpers.ReplaceValueType(
                        ref src,
                        ref src,
                        Unsafe.Reinterpret<T, byte>(oldValue),
                        Unsafe.Reinterpret<T, byte>(newValue),
                        (uint)span.Length);
                    return;
                }
                else if (size == sizeof(ushort))
                {
                    // Use ushort rather than short, as this avoids a sign-extending move.
                    ref ushort src = ref Unsafe.As<T, ushort>(ref MemoryMarshal.GetReference(span));
                    SpanHelpers.ReplaceValueType(
                        ref src,
                        ref src,
                        Unsafe.Reinterpret<T, ushort>(oldValue),
                        Unsafe.Reinterpret<T, ushort>(newValue),
                        (uint)span.Length);
                    return;
                }
                else if (size == sizeof(int))
                {
                    ref int src = ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span));
                    SpanHelpers.ReplaceValueType(
                        ref src,
                        ref src,
                        Unsafe.Reinterpret<T, int>(oldValue),
                        Unsafe.Reinterpret<T, int>(newValue),
                        (uint)span.Length);
                    return;
                }
                else if (size == sizeof(long))
                {
                    ref long src = ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span));
                    SpanHelpers.ReplaceValueType(
                        ref src,
                        ref src,
                        Unsafe.Reinterpret<T, long>(oldValue),
                        Unsafe.Reinterpret<T, long>(newValue),
                        (uint)span.Length);
                    return;
                }
            }

            ReplaceDefaultComparer(span, oldValue, newValue);
            static void ReplaceDefaultComparer(Span<T> span, T oldValue, T newValue)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(span[i], oldValue))
                    {
                        span[i] = newValue;
                    }
                }
            }
        }
        else
        {
            ReplaceComparer(span, oldValue, newValue, comparer);
            static void ReplaceComparer(Span<T> span, T oldValue, T newValue, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;
                for (int i = 0; i < span.Length; i++)
                {
                    if (comparer.Equals(span[i], oldValue))
                    {
                        span[i] = newValue;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Copies <paramref name="source"/> to <paramref name="destination"/>, replacing all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the spans.</typeparam>
    /// <param name="source">The span to copy.</param>
    /// <param name="destination">The span into which the copied and replaced values should be written.</param>
    /// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
    /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <exception cref="ArgumentException">The <paramref name="destination"/> span was shorter than the <paramref name="source"/> span.</exception>
    /// <exception cref="ArgumentException">The <paramref name="source"/> and <paramref name="destination"/> were overlapping but not referring to the same starting location.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Replace<T>(this ReadOnlySpan<T> source, Span<T> destination, T oldValue, T newValue, IEqualityComparer<T>? comparer = null)
    {
        uint length = (uint)source.Length;
        if (length == 0)
        {
            return;
        }

        if (length > (uint)destination.Length)
        {
            ThrowHelper.ThrowArgumentException_DestinationTooShort();
        }

        ref T src = ref MemoryMarshal.GetReference(source);
        ref T dst = ref MemoryMarshal.GetReference(destination);

        int size = Unsafe.SizeOf<T>();

        nint byteOffset = Unsafe.ByteOffset(ref src, ref dst);
        if (byteOffset != 0 &&
            ((nuint)byteOffset < (nuint)((nint)source.Length * size) ||
             (nuint)byteOffset > (nuint)(-((nint)destination.Length * size))))
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);
        }

        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                if (size == sizeof(byte))
                {
                    SpanHelpers.ReplaceValueType(
                        ref Unsafe.As<T, byte>(ref src),
                        ref Unsafe.As<T, byte>(ref dst),
                        Unsafe.Reinterpret<T, byte>(oldValue),
                        Unsafe.Reinterpret<T, byte>(newValue),
                        length);
                    return;
                }
                else if (size == sizeof(ushort))
                {
                    // Use ushort rather than short, as this avoids a sign-extending move.
                    SpanHelpers.ReplaceValueType(
                        ref Unsafe.As<T, ushort>(ref src),
                        ref Unsafe.As<T, ushort>(ref dst),
                        Unsafe.Reinterpret<T, ushort>(oldValue),
                        Unsafe.Reinterpret<T, ushort>(newValue),
                        length);
                    return;
                }
                else if (size == sizeof(int))
                {
                    SpanHelpers.ReplaceValueType(
                        ref Unsafe.As<T, int>(ref src),
                        ref Unsafe.As<T, int>(ref dst),
                        Unsafe.Reinterpret<T, int>(oldValue),
                        Unsafe.Reinterpret<T, int>(newValue),
                        length);
                    return;
                }
                else if (size == sizeof(long))
                {
                    SpanHelpers.ReplaceValueType(
                        ref Unsafe.As<T, long>(ref src),
                        ref Unsafe.As<T, long>(ref dst),
                        Unsafe.Reinterpret<T, long>(oldValue),
                        Unsafe.Reinterpret<T, long>(newValue),
                        length);
                    return;
                }
            }

            ReplaceDefaultComparer(source, destination, oldValue, newValue);
            static void ReplaceDefaultComparer(ReadOnlySpan<T> source, Span<T> destination, T oldValue, T newValue)
            {
                destination = destination.Slice(0, source.Length);

                for (int i = 0; i < source.Length; i++)
                {
                    destination[i] = EqualityComparer<T>.Default.Equals(source[i], oldValue) ? newValue : source[i];
                }
            }
        }
        else
        {
            ReplaceComparer(source, destination, oldValue, newValue, comparer);
            static void ReplaceComparer(ReadOnlySpan<T> source, Span<T> destination, T oldValue, T newValue, IEqualityComparer<T>? comparer)
            {
                destination = destination.Slice(0, source.Length);
                comparer ??= EqualityComparer<T>.Default;

                for (int i = 0; i < source.Length; i++)
                {
                    destination[i] = comparer.Equals(source[i], oldValue) ? newValue : source[i];
                }
            }
        }
    }

    /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value for which to search.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int Count<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.CountValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.CountValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value),
                        span.Length);
                }
                else if (size == sizeof(int))
                {
                    return SpanHelpers.CountValueType(
                        ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, int>(value),
                        span.Length);
                }
                else if (size == sizeof(long))
                {
                    return SpanHelpers.CountValueType(
                        ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, long>(value),
                        span.Length);
                }
            }

            return CountDefaultComparer(span, value);
            static int CountDefaultComparer(ReadOnlySpan<T> span, T value)
            {
                int count = 0;
                for (int i = 0; i < span.Length; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(span[i], value))
                    {
                        count++;
                    }
                }

                return count;
            }
        }
        else
        {
            return CountComparer(span, value, comparer);
            static int CountComparer(ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer)
            {
                comparer ??= EqualityComparer<T>.Default;

                int count = 0;
                for (int i = 0; i < span.Length; i++)
                {
                    if (comparer.Equals(span[i], value))
                    {
                        count++;
                    }
                }

                return count;
            }
        }
    }

    /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value for which to search.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
    public static int Count<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, IEqualityComparer<T>? comparer = null)
    {
        switch (value.Length)
        {
            case 0:
                return 0;

            case 1:
                return Count(span, value[0], comparer);

            default:
                int count = 0;

                int pos;
                while ((pos = span.IndexOf(value, comparer)) >= 0)
                {
                    span = span.Slice(pos + value.Length);
                    count++;
                }

                return count;
        }
    }

    /// <summary>Counts the number of times any of the specified <paramref name="values"/> occurs in the <paramref name="span"/>.</summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The set of values for which to search.</param>
    /// <returns>The number of times any of the <typeparamref name="T"/> elements in <paramref name="values"/> was found in the <paramref name="span"/>.</returns>
    /// <remarks>If <paramref name="values"/> is empty, 0 is returned.</remarks>
    public static int CountAny<T>(this ReadOnlySpan<T> span, params ReadOnlySpan<T> values) where T : IEquatable<T>?
    {
        int count = 0;

        int pos;
        while ((pos = span.IndexOfAny(values)) >= 0)
        {
            count++;
            span = span.Slice(pos + 1);
        }

        return count;
    }

    /// <summary>Counts the number of times any of the specified <paramref name="values"/> occurs in the <paramref name="span"/>.</summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The set of values for which to search.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the
    /// default <see cref="IEqualityComparer{T}"/> for the type of an element.
    /// </param>
    /// <returns>The number of times any of the <typeparamref name="T"/> elements in <paramref name="values"/> was found in the <paramref name="span"/>.</returns>
    /// <remarks>If <paramref name="values"/> is empty, 0 is returned.</remarks>
    public static int CountAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values, IEqualityComparer<T>? comparer = null)
    {
        int count = 0;

        int pos;
        while ((pos = span.IndexOfAny(values, comparer)) >= 0)
        {
            count++;
            span = span.Slice(pos + 1);
        }

        return count;
    }

}

#endif