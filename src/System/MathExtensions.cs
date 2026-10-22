// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides compatibility implementations of mathematical operations.
/// </summary>
public static class MathExtensions
{
#if !NET5_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
    extension(Math)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp(byte value, byte min, byte max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Clamp(short value, short min, short max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long value, long min, long max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        /// <summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
        /// <param name="value">The value to be clamped.</param>
        /// <param name="min">The lower bound of the result.</param>
        /// <param name="max">The upper bound of the result.</param>
        /// <returns>
        ///   <paramref name="value" /> if <paramref name="min" /> \u2264 <paramref name="value" /> \u2264 <paramref name="max" />.
        ///
        ///   -or-
        ///
        ///   <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />.
        ///
        ///   -or-
        ///
        ///   <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Clamp(nint value, nint min, nint max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Clamp(sbyte value, sbyte min, sbyte max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Clamp(ushort value, ushort min, ushort max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(uint value, uint min, uint max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Clamp(ulong value, ulong min, ulong max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        /// <summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
        /// <param name="value">The value to be clamped.</param>
        /// <param name="min">The lower bound of the result.</param>
        /// <param name="max">The upper bound of the result.</param>
        /// <returns>
        ///   <paramref name="value" /> if <paramref name="min" /> \u2264 <paramref name="value" /> \u2264 <paramref name="max" />.
        ///
        ///   -or-
        ///
        ///   <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />.
        ///
        ///   -or-
        ///
        ///   <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Clamp(nuint value, nuint min, nuint max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }
    }

    [DoesNotReturn]
    internal static void ThrowMinMaxException<T>(T min, T max)
    {
        throw new ArgumentException(SR.Format(SR.Argument_MinMaxValue, min, max));
    }
#endif
}
