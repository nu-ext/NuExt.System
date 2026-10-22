// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides methods for converting and validating ASCII hexadecimal characters.
/// </summary>
public static class HexConverter
{
    /// <summary>
    /// Converts an ASCII hexadecimal character to its numeric value.
    /// </summary>
    /// <param name="c">The character code to convert.</param>
    /// <returns>
    /// A value from 0 through 15 if <paramref name="c"/> represents an ASCII hexadecimal
    /// character; otherwise, 255.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromChar(int c)
    {
        return (uint)c < (uint)CharToHexLookup.Length ? CharToHexLookup[c] : 0xFF;
    }

    /// <summary>
    /// Converts an uppercase ASCII hexadecimal character to its numeric value.
    /// </summary>
    /// <param name="c">The character code to convert.</param>
    /// <returns>
    /// A value from 0 through 15 if <paramref name="c"/> represents an uppercase ASCII
    /// hexadecimal character; otherwise, 255.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromUpperChar(int c)
    {
        return (uint)c <= 'F' ? CharToHexLookup[c] : 0xFF;
    }

    /// <summary>
    /// Converts a lowercase ASCII hexadecimal character to its numeric value.
    /// </summary>
    /// <param name="c">The character code to convert.</param>
    /// <returns>
    /// A value from 0 through 15 if <paramref name="c"/> represents a lowercase ASCII
    /// hexadecimal character; otherwise, 255.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromLowerChar(int c)
    {
        if ((uint)(c - '0') <= ('9' - '0'))
        {
            return c - '0';
        }

        if ((uint)(c - 'a') <= ('f' - 'a'))
        {
            return c - 'a' + 10;
        }

        return 0xFF;
    }

    /// <summary>
    /// Determines whether the specified character is an ASCII hexadecimal character.
    /// </summary>
    /// <param name="c">The character code to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="c"/> is an ASCII hexadecimal character;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHexChar(int c)
    {
        if (IntPtr.Size == 8)
        {
            // This code path, when used, has no branches and doesn't depend on cache hits,
            // so it's faster and does not vary in speed depending on input data distribution.
            // We only use this logic on 64-bit systems, as using 64 bit values would otherwise
            // be much slower than just using the lookup table anyway (no hardware support).
            // The magic constant 18428868213665201664 is a 64 bit value containing 1s at the
            // indices corresponding to all the valid hex characters (ie. "0123456789ABCDEFabcdef")
            // minus 48 (ie. '0'), and backwards (so from the most significant bit and downwards).
            // The offset of 48 for each bit is necessary so that the entire range fits in 64 bits.
            // First, we subtract '0' to the input digit (after casting to uint to account for any
            // negative inputs). Note that even if this subtraction underflows, this happens before
            // the result is zero-extended to ulong, meaning that `i` will always have upper 32 bits
            // equal to 0. We then left shift the constant with this offset, and apply a bitmask that
            // has the highest bit set (the sign bit) if and only if `c` is in the ['0', '0' + 64) range.
            // Then we only need to check whether this final result is less than 0: this will only be
            // the case if both `i` was in fact the index of a set bit in the magic constant, and also
            // `c` was in the allowed range (this ensures that false positive bit shifts are ignored).
            ulong i = (uint)c - '0';
            ulong shift = 18428868213665201664UL << (int)i;
            ulong mask = i - 64;

            return (long)(shift & mask) < 0 ? true : false;
        }

        return FromChar(c) != 0xFF;
    }

    /// <summary>
    /// Determines whether the specified character is an uppercase ASCII hexadecimal character.
    /// </summary>
    /// <param name="c">The character code to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="c"/> is an ASCII digit or an uppercase
    /// hexadecimal letter; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHexUpperChar(int c)
    {
        return ((uint)(c - '0') <= 9) || ((uint)(c - 'A') <= ('F' - 'A'));
    }

    /// <summary>
    /// Determines whether the specified character is a lowercase ASCII hexadecimal character.
    /// </summary>
    /// <param name="c">The character code to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="c"/> is an ASCII digit or a lowercase
    /// hexadecimal letter; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHexLowerChar(int c)
    {
        return ((uint)(c - '0') <= 9) || ((uint)(c - 'a') <= ('f' - 'a'));
    }

    /// <summary>Map from an ASCII char to its hex value, e.g. arr['b'] == 11. 0xFF means it's not a hex digit.</summary>
    public static ReadOnlySpan<byte> CharToHexLookup =>
    [
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 15
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 31
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 47
        0x0,  0x1,  0x2,  0x3,  0x4,  0x5,  0x6,  0x7,  0x8,  0x9,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 63
        0xFF, 0xA,  0xB,  0xC,  0xD,  0xE,  0xF,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 79
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 95
        0xFF, 0xa,  0xb,  0xc,  0xd,  0xe,  0xf,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 111
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 127
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 143
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 159
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 175
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 191
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 207
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 223
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 239
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF  // 255
    ];
}
