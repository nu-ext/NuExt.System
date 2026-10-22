// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.
#if !NET8_0_OR_GREATER
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

partial class CompatMemoryExtensions
{
    /// <inheritdoc cref="ContainsAny{T}(ReadOnlySpan{T}, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAny<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).ContainsAny(value0, value1);

    /// <inheritdoc cref="ContainsAny{T}(ReadOnlySpan{T}, T, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAny<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).ContainsAny(value0, value1, value2);

    /// <inheritdoc cref="ContainsAny{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAny<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).ContainsAny(values);

    /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlySpan{T}, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAnyExcept<T>(this Span<T> span, T value) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).ContainsAnyExcept(value);

    /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlySpan{T}, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAnyExcept<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).ContainsAnyExcept(value0, value1);

    /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlySpan{T}, T, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAnyExcept<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).ContainsAnyExcept(value0, value1, value2);

    /// <inheritdoc cref="ContainsAnyExcept{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAnyExcept<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).ContainsAnyExcept(values);

    /// <inheritdoc cref="ContainsAnyInRange{T}(ReadOnlySpan{T}, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAnyInRange<T>(this Span<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
        ((ReadOnlySpan<T>)span).ContainsAnyInRange(lowInclusive, highInclusive);

    /// <inheritdoc cref="ContainsAnyExceptInRange{T}(ReadOnlySpan{T}, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static bool ContainsAnyExceptInRange<T>(this Span<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
        ((ReadOnlySpan<T>)span).ContainsAnyExceptInRange(lowInclusive, highInclusive);

    /// <summary>
    /// Searches for any occurrence of the specified <paramref name="value0"/> or <paramref name="value1"/>, and returns true if found. If not found, returns false.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">One of the values to search for.</param>
    /// <param name="value1">One of the values to search for.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
        span.IndexOfAny(value0, value1) >= 0;

    /// <summary>
    /// Searches for any occurrence of the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, and returns true if found. If not found, returns false.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">One of the values to search for.</param>
    /// <param name="value1">One of the values to search for.</param>
    /// <param name="value2">One of the values to search for.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
        span.IndexOfAny(value0, value1, value2) >= 0;

    /// <summary>
    /// Searches for any occurrence of any of the specified <paramref name="values"/> and returns true if found. If not found, returns false.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The set of values to search for.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>? =>
        span.IndexOfAny(values) >= 0;

    /// <summary>
    /// Searches for any value other than the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value">A value to avoid.</param>
    /// <returns>
    /// True if any value other than <paramref name="value"/> is present in the span.
    /// If all of the values are <paramref name="value"/>, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>? =>
        span.IndexOfAnyExcept(value) >= 0;

    /// <summary>
    /// Searches for any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid.</param>
    /// <returns>
    /// True if any value other than <paramref name="value0"/> and <paramref name="value1"/> is present in the span.
    /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>? =>
        span.IndexOfAnyExcept(value0, value1) >= 0;

    /// <summary>
    /// Searches for any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid.</param>
    /// <param name="value2">A value to avoid.</param>
    /// <returns>
    /// True if any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/> is present in the span.
    /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
        span.IndexOfAnyExcept(value0, value1, value2) >= 0;

    /// <summary>
    /// Searches for any value other than the specified <paramref name="values"/>.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The values to avoid.</param>
    /// <returns>
    /// True if any value other than those in <paramref name="values"/> is present in the span.
    /// If all of the values are in <paramref name="values"/>, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>? =>
        span.IndexOfAnyExcept(values) >= 0;

    /// <summary>
    /// Searches for any value in the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive, and returns true if found. If not found, returns false.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="lowInclusive">A lower bound, inclusive, of the range for which to search.</param>
    /// <param name="highInclusive">A upper bound, inclusive, of the range for which to search.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyInRange<T>(this ReadOnlySpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
        span.IndexOfAnyInRange(lowInclusive, highInclusive) >= 0;

    /// <summary>
    /// Searches for any value outside of the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.
    /// </summary>
    /// <param name="span">The span to search.</param>
    /// <param name="lowInclusive">A lower bound, inclusive, of the excluded range.</param>
    /// <param name="highInclusive">A upper bound, inclusive, of the excluded range.</param>
    /// <returns>
    /// True if any value other than those in the specified range is present in the span.
    /// If all of the values are inside of the specified range, returns false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyExceptInRange<T>(this ReadOnlySpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
        span.IndexOfAnyExceptInRange(lowInclusive, highInclusive) >= 0;

    /// <inheritdoc cref="IndexOfAnyInRange{T}(ReadOnlySpan{T}, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static int IndexOfAnyInRange<T>(this Span<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
        ((ReadOnlySpan<T>)span).IndexOfAnyInRange(lowInclusive, highInclusive);

    /// <summary>Searches for the first index of any value in the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="lowInclusive">A lower bound, inclusive, of the range for which to search.</param>
    /// <param name="highInclusive">A upper bound, inclusive, of the range for which to search.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value in the specified range.
    /// If all of the values are outside of the specified range, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAnyInRange<T>(this ReadOnlySpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T>
    {
        if (lowInclusive is null || highInclusive is null)
        {
            ThrowNullLowHighInclusive(lowInclusive, highInclusive);
        }

        // When highInclusive < lowInclusive, the range is invalid and no values can match.
        if (lowInclusive.CompareTo(highInclusive) > 0)
        {
            return -1;
        }

        return SpanHelpers.IndexOfAnyInRange(ref MemoryMarshal.GetReference(span), lowInclusive, highInclusive, span.Length);
    }

    /// <inheritdoc cref="IndexOfAnyExceptInRange{T}(ReadOnlySpan{T}, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static int IndexOfAnyExceptInRange<T>(this Span<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
        ((ReadOnlySpan<T>)span).IndexOfAnyExceptInRange(lowInclusive, highInclusive);

    /// <summary>Searches for the first index of any value outside of the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="lowInclusive">A lower bound, inclusive, of the excluded range.</param>
    /// <param name="highInclusive">A upper bound, inclusive, of the excluded range.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value outside of the specified range.
    /// If all of the values are inside of the specified range, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAnyExceptInRange<T>(this ReadOnlySpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T>
    {
        if (lowInclusive is null || highInclusive is null)
        {
            ThrowNullLowHighInclusive(lowInclusive, highInclusive);
        }

        // When highInclusive < lowInclusive, the range is invalid and all values are outside it.
        if (lowInclusive.CompareTo(highInclusive) > 0)
        {
            return span.IsEmpty ? -1 : 0;
        }

        return SpanHelpers.IndexOfAnyExceptInRange(ref MemoryMarshal.GetReference(span), lowInclusive, highInclusive, span.Length);
    }

    /// <inheritdoc cref="LastIndexOfAnyInRange{T}(ReadOnlySpan{T}, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static int LastIndexOfAnyInRange<T>(this Span<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
        ((ReadOnlySpan<T>)span).LastIndexOfAnyInRange(lowInclusive, highInclusive);

    /// <summary>Searches for the last index of any value in the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="lowInclusive">A lower bound, inclusive, of the range for which to search.</param>
    /// <param name="highInclusive">A upper bound, inclusive, of the range for which to search.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value in the specified range.
    /// If all of the values are outside of the specified range, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOfAnyInRange<T>(this ReadOnlySpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T>
    {
        if (lowInclusive is null || highInclusive is null)
        {
            ThrowNullLowHighInclusive(lowInclusive, highInclusive);
        }

        // When highInclusive < lowInclusive, the range is invalid and no values can match.
        if (lowInclusive.CompareTo(highInclusive) > 0)
        {
            return -1;
        }

        return SpanHelpers.LastIndexOfAnyInRange(ref MemoryMarshal.GetReference(span), lowInclusive, highInclusive, span.Length);
    }

    /// <inheritdoc cref="LastIndexOfAnyExceptInRange{T}(ReadOnlySpan{T}, T, T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(-1)]
    public static int LastIndexOfAnyExceptInRange<T>(this Span<T> span, T lowInclusive, T highInclusive) where T : IComparable<T> =>
        ((ReadOnlySpan<T>)span).LastIndexOfAnyExceptInRange(lowInclusive, highInclusive);

    /// <summary>Searches for the last index of any value outside of the range between <paramref name="lowInclusive"/> and <paramref name="highInclusive"/>, inclusive.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="lowInclusive">A lower bound, inclusive, of the excluded range.</param>
    /// <param name="highInclusive">A upper bound, inclusive, of the excluded range.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value outside of the specified range.
    /// If all of the values are inside of the specified range, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOfAnyExceptInRange<T>(this ReadOnlySpan<T> span, T lowInclusive, T highInclusive) where T : IComparable<T>
    {
        if (lowInclusive is null || highInclusive is null)
        {
            ThrowNullLowHighInclusive(lowInclusive, highInclusive);
        }

        // When highInclusive < lowInclusive, the range is invalid and all values are outside it.
        if (lowInclusive.CompareTo(highInclusive) > 0)
        {
            return span.Length - 1;
        }

        return SpanHelpers.LastIndexOfAnyExceptInRange(ref MemoryMarshal.GetReference(span), lowInclusive, highInclusive, span.Length);
    }

    /// <summary>Throws an <see cref="ArgumentNullException"/> for <paramref name="lowInclusive"/> or <paramref name="highInclusive"/> being null.</summary>
    [DoesNotReturn]
    private static void ThrowNullLowHighInclusive<T>(T? lowInclusive, T? highInclusive)
    {
        Debug.Assert(lowInclusive is null || highInclusive is null);
        throw new ArgumentNullException(lowInclusive is null ? nameof(lowInclusive) : nameof(highInclusive));
    }

    /// <summary>
    /// Replaces all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span in which the elements should be replaced.</param>
    /// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
    /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Replace<T>(this Span<T> span, T oldValue, T newValue) where T : IEquatable<T>?
    {
        uint length = (uint)span.Length;

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
                    length);
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
                    length);
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
                    length);
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
                    length);
                return;
            }
        }

        ref T src2 = ref MemoryMarshal.GetReference(span);
        SpanHelpers.Replace(ref src2, ref src2, oldValue, newValue, length);
    }

    /// <summary>
    /// Copies <paramref name="source"/> to <paramref name="destination"/>, replacing all occurrences of <paramref name="oldValue"/> with <paramref name="newValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the spans.</typeparam>
    /// <param name="source">The span to copy.</param>
    /// <param name="destination">The span into which the copied and replaced values should be written.</param>
    /// <param name="oldValue">The value to be replaced with <paramref name="newValue"/>.</param>
    /// <param name="newValue">The value to replace all occurrences of <paramref name="oldValue"/>.</param>
    /// <exception cref="ArgumentException">The <paramref name="destination"/> span was shorter than the <paramref name="source"/> span.</exception>
    /// <exception cref="ArgumentException">The <paramref name="source"/> and <paramref name="destination"/> were overlapping but not referring to the same starting location.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Replace<T>(this ReadOnlySpan<T> source, Span<T> destination, T oldValue, T newValue) where T : IEquatable<T>?
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

        SpanHelpers.Replace(ref src, ref dst, oldValue, newValue, length);
    }

    /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value for which to search.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
    [OverloadResolutionPriority(-1)]
    public static int Count<T>(this Span<T> span, T value) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).Count(value);

    /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value for which to search.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>?
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

        return SpanHelpers.Count(
            ref MemoryMarshal.GetReference(span),
            value,
            span.Length);
    }

    /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value for which to search.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
    [OverloadResolutionPriority(-1)]
    public static int Count<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).Count(value);

    /// <summary>Counts the number of times the specified <paramref name="value"/> occurs in the <paramref name="span"/>.</summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value for which to search.</param>
    /// <returns>The number of times <paramref name="value"/> was found in the <paramref name="span"/>.</returns>
    public static int Count<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>?
    {
        switch (value.Length)
        {
            case 0:
                return 0;

            case 1:
                return Count(span, value[0]);

            default:
                int count = 0;

                int pos;
                while ((pos = span.IndexOf(value)) >= 0)
                {
                    span = span.Slice(pos + value.Length);
                    count++;
                }

                return count;
        }
    }
}
#endif