// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.
#if !NET7_0_OR_GREATER

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

partial class CompatMemoryExtensions
{
    /// <summary>Searches for the first index of any value other than the specified <paramref name="value"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">A value to avoid.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value"/>.
    /// If all of the values are <paramref name="value"/>, returns -1.
    /// </returns>
    [OverloadResolutionPriority(-1)]
    public static int IndexOfAnyExcept<T>(this Span<T> span, T value) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).IndexOfAnyExcept(value);

    /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
    /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
    /// </returns>
    [OverloadResolutionPriority(-1)]
    public static int IndexOfAnyExcept<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).IndexOfAnyExcept(value0, value1);

    /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <param name="value2">A value to avoid</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
    /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>, returns -1.
    /// </returns>
    [OverloadResolutionPriority(-1)]
    public static int IndexOfAnyExcept<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).IndexOfAnyExcept(value0, value1, value2);

    /// <summary>Searches for the first index of any value other than the specified <paramref name="values"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The values to avoid.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
    /// If all of the values are in <paramref name="values"/>, returns -1.
    /// </returns>
    [OverloadResolutionPriority(-1)]
    public static int IndexOfAnyExcept<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).IndexOfAnyExcept(values);

    /// <summary>Searches for the first index of any value other than the specified <paramref name="value"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">A value to avoid.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value"/>.
    /// If all of the values are <paramref name="value"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>?
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

        return SpanHelpers.IndexOfAnyExcept(ref MemoryMarshal.GetReference(span), value, span.Length);
    }

    /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
    /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>?
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

        return SpanHelpers.IndexOfAnyExcept(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
    }

    /// <summary>Searches for the first index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <param name="value2">A value to avoid</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
    /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>?
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

        return SpanHelpers.IndexOfAnyExcept(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2, T value3) where T : IEquatable<T>?
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
                    Unsafe.Reinterpret<T, byte>(value3),
                    span.Length);
            }
            else if (size == sizeof(short))
            {
                return SpanHelpers.IndexOfAnyExceptValueType(
                    ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                    Unsafe.Reinterpret<T, short>(value0),
                    Unsafe.Reinterpret<T, short>(value1),
                    Unsafe.Reinterpret<T, short>(value2),
                    Unsafe.Reinterpret<T, short>(value3),
                    span.Length);
            }
        }

        return SpanHelpers.IndexOfAnyExcept(ref MemoryMarshal.GetReference(span), value0, value1, value2, value3, span.Length);
    }

    /// <summary>Searches for the first index of any value other than the specified <paramref name="values"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The values to avoid.</param>
    /// <returns>
    /// The index in the span of the first occurrence of any value other than those in <paramref name="values"/>.
    /// If all of the values are in <paramref name="values"/>, returns -1.
    /// </returns>
    public static unsafe int IndexOfAnyExcept<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>?
    {
        switch (values.Length)
        {
            case 0:
                // If the span is empty, we want to return -1.
                // If the span is non-empty, we want to return the index of the first char that's not in the empty set,
                // which is every character, and so the first char in the span.
                return span.IsEmpty ? -1 : 0;

            case 1:
                return IndexOfAnyExcept(span, values[0]);

            case 2:
                return IndexOfAnyExcept(span, values[0], values[1]);

            case 3:
                return IndexOfAnyExcept(span, values[0], values[1], values[2]);

            case 4:
                return IndexOfAnyExcept(span, values[0], values[1], values[2], values[3]);

            default:
                int size = Unsafe.SizeOf<T>();
                if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
                {
                    if (size == sizeof(byte) && values.Length == 5)
                    {
                        ref byte valuesRef = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values));

                        return SpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                            valuesRef,
                            Unsafe.Add(ref valuesRef, 1),
                            Unsafe.Add(ref valuesRef, 2),
                            Unsafe.Add(ref valuesRef, 3),
                            Unsafe.Add(ref valuesRef, 4),
                            span.Length);
                    }
                    else if (size == sizeof(short) && values.Length == 5)
                    {
                        ref short valuesRef = ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(values));

                        return SpanHelpers.IndexOfAnyExceptValueType(
                            ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                            valuesRef,
                            Unsafe.Add(ref valuesRef, 1),
                            Unsafe.Add(ref valuesRef, 2),
                            Unsafe.Add(ref valuesRef, 3),
                            Unsafe.Add(ref valuesRef, 4),
                            span.Length);
                    }
                }

                if (RuntimeHelpers.IsKnownBitwiseEquatable<T>() && size == sizeof(char))
                {
                    return SpanHelpers.IndexOfAnyExcept(
                        ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(values)),
                        values.Length);
                }

                for (int i = 0; i < span.Length; i++)
                {
                    if (!values.Contains(span[i]))
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
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value"/>.
    /// If all of the values are <paramref name="value"/>, returns -1.
    /// </returns>
    [OverloadResolutionPriority(-1)]
    public static int LastIndexOfAnyExcept<T>(this Span<T> span, T value) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).LastIndexOfAnyExcept(value);

    /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
    /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
    /// </returns>
    [OverloadResolutionPriority(-1)]
    public static int LastIndexOfAnyExcept<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).LastIndexOfAnyExcept(value0, value1);

    /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <param name="value2">A value to avoid</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
    /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
    /// </returns>
    [OverloadResolutionPriority(-1)]
    public static int LastIndexOfAnyExcept<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).LastIndexOfAnyExcept(value0, value1, value2);

    /// <summary>Searches for the last index of any value other than the specified <paramref name="values"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The values to avoid.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than those in <paramref name="values"/>.
    /// If all of the values are in <paramref name="values"/>, returns -1.
    /// </returns>
    [OverloadResolutionPriority(-1)]
    public static int LastIndexOfAnyExcept<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>? =>
        ((ReadOnlySpan<T>)span).LastIndexOfAnyExcept(values);

    /// <summary>Searches for the last index of any value other than the specified <paramref name="value"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">A value to avoid.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value"/>.
    /// If all of the values are <paramref name="value"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>?
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

        return SpanHelpers.LastIndexOfAnyExcept(ref MemoryMarshal.GetReference(span), value, span.Length);
    }

    /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/> or <paramref name="value1"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value0"/> and <paramref name="value1"/>.
    /// If all of the values are <paramref name="value0"/> or <paramref name="value1"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>?
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

        return SpanHelpers.LastIndexOfAnyExcept(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
    }

    /// <summary>Searches for the last index of any value other than the specified <paramref name="value0"/>, <paramref name="value1"/>, or <paramref name="value2"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value0">A value to avoid.</param>
    /// <param name="value1">A value to avoid</param>
    /// <param name="value2">A value to avoid</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>.
    /// If all of the values are <paramref name="value0"/>, <paramref name="value1"/>, and <paramref name="value2"/>, returns -1.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>?
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

        return SpanHelpers.LastIndexOfAnyExcept(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2, T value3) where T : IEquatable<T>?
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
                    Unsafe.Reinterpret<T, byte>(value3),
                    span.Length);
            }
            else if (size == sizeof(short))
            {
                return SpanHelpers.LastIndexOfAnyExceptValueType(
                    ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                    Unsafe.Reinterpret<T, short>(value0),
                    Unsafe.Reinterpret<T, short>(value1),
                    Unsafe.Reinterpret<T, short>(value2),
                    Unsafe.Reinterpret<T, short>(value3),
                    span.Length);
            }
        }

        return SpanHelpers.LastIndexOfAnyExcept(ref MemoryMarshal.GetReference(span), value0, value1, value2, value3, span.Length);
    }

    /// <summary>Searches for the last index of any value other than the specified <paramref name="values"/>.</summary>
    /// <typeparam name="T">The type of the span and values.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="values">The values to avoid.</param>
    /// <returns>
    /// The index in the span of the last occurrence of any value other than those in <paramref name="values"/>.
    /// If all of the values are in <paramref name="values"/>, returns -1.
    /// </returns>
    public static unsafe int LastIndexOfAnyExcept<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>?
    {
        switch (values.Length)
        {
            case 0:
                // If the span is empty, we want to return -1.
                // If the span is non-empty, we want to return the index of the last char that's not in the empty set,
                // which is every character, and so the last char in the span.
                // Either way, we want to return span.Length - 1.
                return span.Length - 1;

            case 1:
                return LastIndexOfAnyExcept(span, values[0]);

            case 2:
                return LastIndexOfAnyExcept(span, values[0], values[1]);

            case 3:
                return LastIndexOfAnyExcept(span, values[0], values[1], values[2]);

            case 4:
                return LastIndexOfAnyExcept(span, values[0], values[1], values[2], values[3]);

            default:
                int size = Unsafe.SizeOf<T>();
                if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
                {
                    if (size == sizeof(byte) && values.Length == 5)
                    {
                        ref byte valuesRef = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values));

                        return SpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                            valuesRef,
                            Unsafe.Add(ref valuesRef, 1),
                            Unsafe.Add(ref valuesRef, 2),
                            Unsafe.Add(ref valuesRef, 3),
                            Unsafe.Add(ref valuesRef, 4),
                            span.Length);
                    }
                    else if (size == sizeof(short) && values.Length == 5)
                    {
                        ref short valuesRef = ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(values));

                        return SpanHelpers.LastIndexOfAnyExceptValueType(
                            ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                            valuesRef,
                            Unsafe.Add(ref valuesRef, 1),
                            Unsafe.Add(ref valuesRef, 2),
                            Unsafe.Add(ref valuesRef, 3),
                            Unsafe.Add(ref valuesRef, 4),
                            span.Length);
                    }
                }

                if (RuntimeHelpers.IsKnownBitwiseEquatable<T>() && size == sizeof(char))
                {
                    return SpanHelpers.LastIndexOfAnyExcept(
                        ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)),
                        span.Length,
                        ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(values)),
                        values.Length);
                }

                for (int i = span.Length - 1; i >= 0; i--)
                {
                    if (!values.Contains(span[i]))
                    {
                        return i;
                    }
                }

                return -1;
        }
    }

    /// <summary>Finds the length of any common prefix shared between <paramref name="span"/> and <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the elements in the spans.</typeparam>
    /// <param name="span">The first sequence to compare.</param>
    /// <param name="other">The second sequence to compare.</param>
    /// <returns>The length of the common prefix shared by the two spans.  If there's no shared prefix, 0 is returned.</returns>
    [OverloadResolutionPriority(-1)]
    public static int CommonPrefixLength<T>(this Span<T> span, ReadOnlySpan<T> other) =>
        ((ReadOnlySpan<T>)span).CommonPrefixLength(other);

    /// <summary>Finds the length of any common prefix shared between <paramref name="span"/> and <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the elements in the spans.</typeparam>
    /// <param name="span">The first sequence to compare.</param>
    /// <param name="other">The second sequence to compare.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>The length of the common prefix shared by the two spans.  If there's no shared prefix, 0 is returned.</returns>
    [OverloadResolutionPriority(-1)]
    public static int CommonPrefixLength<T>(this Span<T> span, ReadOnlySpan<T> other, IEqualityComparer<T>? comparer) =>
        ((ReadOnlySpan<T>)span).CommonPrefixLength(other, comparer);

    /// <summary>Finds the length of any common prefix shared between <paramref name="span"/> and <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the elements in the spans.</typeparam>
    /// <param name="span">The first sequence to compare.</param>
    /// <param name="other">The second sequence to compare.</param>
    /// <returns>The length of the common prefix shared by the two spans.  If there's no shared prefix, 0 is returned.</returns>
    public static unsafe int CommonPrefixLength<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other)
    {
        if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
        {
            nuint length = (nuint)Math.Min((nuint)(uint)span.Length, (nuint)(uint)other.Length);
            nuint size = (uint)Unsafe.SizeOf<T>();
            nuint index = SpanHelpers.CommonPrefixLength(
                ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)),
                length * size);

            // A byte-wise comparison in CommonPrefixLength can be used for multi-byte types,
            // that are bitwise-equatable, too. In order to get the correct index in terms of type T
            // of the first mismatch, integer division by the size of T is used.
            //
            // Example for short:
            // index (byte-based):   b-1,  b,    b+1,    b+2,  b+3
            // index (short-based):  s-1,  s,            s+1
            // byte sequence 1:    { ..., [0x42, 0x43], [0x37, 0x38], ... }
            // byte sequence 2:    { ..., [0x42, 0x43], [0x37, 0xAB], ... }
            // So the mismatch is a byte-index b+3, which gives integer divided by the size of short:
            // 3 / 2 = 1, thus the expected index short-based.
            return (int)(index / size);
        }

        // Shrink one of the spans if necessary to ensure they're both the same length. We can then iterate until
        // the Length of one of them and at least have bounds checks removed from that one.
        SliceLongerSpanToMatchShorterLength(ref span, ref other);

        // Find the first element pairwise that is not equal, and return its index as the length
        // of the sequence before it that matches.
        for (int i = 0; i < span.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(span[i], other[i]))
            {
                return i;
            }
        }

        return span.Length;
    }

    /// <summary>Determines the length of any common prefix shared between <paramref name="span"/> and <paramref name="other"/>.</summary>
    /// <typeparam name="T">The type of the elements in the sequences.</typeparam>
    /// <param name="span">The first sequence to compare.</param>
    /// <param name="other">The second sequence to compare.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>The length of the common prefix shared by the two spans.  If there's no shared prefix, 0 is returned.</returns>
    public static int CommonPrefixLength<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other, IEqualityComparer<T>? comparer)
    {
        // If the comparer is null or the default, and T is a value type, we want to use EqualityComparer<T>.Default.Equals
        // directly to enable devirtualization.  The non-comparer overload already does so, so just use it.
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            return CommonPrefixLength(span, other);
        }

        // Shrink one of the spans if necessary to ensure they're both the same length. We can then iterate until
        // the Length of one of them and at least have bounds checks removed from that one.
        SliceLongerSpanToMatchShorterLength(ref span, ref other);

        // Ensure we have a comparer, then compare the spans.
        comparer ??= EqualityComparer<T>.Default;
        for (int i = 0; i < span.Length; i++)
        {
            if (!comparer.Equals(span[i], other[i]))
            {
                return i;
            }
        }

        return span.Length;
    }

    /// <summary>Determines if one span is longer than the other, and slices the longer one to match the length of the shorter.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SliceLongerSpanToMatchShorterLength<T>(ref ReadOnlySpan<T> span, ref ReadOnlySpan<T> other)
    {
        if (other.Length > span.Length)
        {
            other = other.Slice(0, span.Length);
        }

        span = span.Slice(0, other.Length);
    }

}
#endif