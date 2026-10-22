using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System;

/// <summary>
/// Provides compatibility and utility methods for working with strings.
/// </summary>
public static class StringExtensions
{
    internal const int StackallocCharBufferSizeLimit = 256;

    extension(String)
    {
#if !NET
        public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1)
        {
            int length = checked(str0.Length + str1.Length);
            if (length == 0)
            {
                return string.Empty;
            }

            var builder = new ValueStringBuilder(length);
            builder.Append(str0);
            builder.Append(str1);
            return builder.ToString();
        }

        public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
        {
            int length = checked(str0.Length + str1.Length + str2.Length);
            if (length == 0)
            {
                return string.Empty;
            }

            var builder = new ValueStringBuilder(length);
            builder.Append(str0);
            builder.Append(str1);
            builder.Append(str2);
            return builder.ToString();
        }

        public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3)
        {
            int length = checked(str0.Length + str1.Length + str2.Length + str3.Length);
            if (length == 0)
            {
                return string.Empty;
            }

            var builder = new ValueStringBuilder(length);
            builder.Append(str0);
            builder.Append(str1);
            builder.Append(str2);
            builder.Append(str3);
            return builder.ToString();
        }

#endif
        /// <summary>
        /// Concatenates five read-only character spans.
        /// </summary>
        /// <param name="str0">The first span to concatenate.</param>
        /// <param name="str1">The second span to concatenate.</param>
        /// <param name="str2">The third span to concatenate.</param>
        /// <param name="str3">The fourth span to concatenate.</param>
        /// <param name="str4">The fifth span to concatenate.</param>
        /// <returns>The concatenated contents of the specified spans.</returns>
        public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3, ReadOnlySpan<char> str4)
        {
            int length = checked(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length);
            if (length == 0)
            {
                return string.Empty;
            }

            var builder = new ValueStringBuilder(length);
            builder.Append(str0);
            builder.Append(str1);
            builder.Append(str2);
            builder.Append(str3);
            builder.Append(str4);
            return builder.ToString();
        }

#if !NET9_0_OR_GREATER
        /// <summary>
        /// Concatenates the string representations of the elements in a specified span of objects.
        /// </summary>
        /// <param name="args">A span of objects that contains the elements to concatenate.</param>
        /// <returns>The concatenated string representations of the values of the elements in <paramref name="args"/>.</returns>
        public static string Concat(params ReadOnlySpan<object?> args)
        {
            if (args.Length <= 1)
            {
                return args.IsEmpty ?
                    string.Empty :
                    args[0]?.ToString() ?? string.Empty;
            }

            // We need to get an intermediary string array
            // to fill with each of the args' ToString(),
            // and then just concat that in one operation.

            // This way we avoid any intermediary string representations,
            // or buffer resizing if we use StringBuilder (although the
            // latter case is partially alleviated due to StringBuilder's
            // linked-list style implementation)

            var strings = new string[args.Length];

            int totalLength = 0;

            for (int i = 0; i < args.Length; i++)
            {
                object? value = args[i];

                string toString = value?.ToString() ?? string.Empty; // We need to handle both the cases when value or value.ToString() is null
                strings[i] = toString;

                totalLength += toString.Length;

                if (totalLength < 0) // Check for a positive overflow
                {
                    ThrowHelper.ThrowOutOfMemoryException_StringTooLong();
                }
            }

            // If all of the ToStrings are null/empty, just return string.Empty
            if (totalLength == 0)
            {
                return string.Empty;
            }

            var builder = new ValueStringBuilder(totalLength);
            int position = 0; // How many characters we've copied so far

            for (int i = 0; i < strings.Length; i++)
            {
                string s = strings[i];

                Debug.Assert(s != null);
                Debug.Assert(position <= totalLength - s!.Length, "We didn't allocate enough space for the result string!");

                builder.Append(s);
                position += s.Length;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Concatenates the elements of a specified span of <see cref="string"/>.
        /// </summary>
        /// <param name="values">A span of <see cref="string"/> instances.</param>
        /// <returns>The concatenated elements of <paramref name="values"/>.</returns>
        public static string Concat(params ReadOnlySpan<string?> values)
        {
            if (values.Length <= 1)
            {
                return values.IsEmpty ?
                    string.Empty :
                    values[0] ?? string.Empty;
            }

            // It's possible that the input values array could be changed concurrently on another
            // thread, such that we can't trust that each read of values[i] will be equivalent.
            // Worst case, we can make a defensive copy of the array and use that, but we first
            // optimistically try the allocation and copies assuming that the array isn't changing,
            // which represents the 99.999% case, in particular since string.Concat is used for
            // string concatenation by the languages, with the input array being a params array.

            // Sum the lengths of all input strings
            long totalLengthLong = 0;
            for (int i = 0; i < values.Length; i++)
            {
                string? value = values[i];
                if (value != null)
                {
                    totalLengthLong += value.Length;
                }
            }

            // If it's too long, fail, or if it's empty, return an empty string.
            if (totalLengthLong > int.MaxValue)
            {
                ThrowHelper.ThrowOutOfMemoryException_StringTooLong();
            }
            int totalLength = (int)totalLengthLong;
            if (totalLength == 0)
            {
                return string.Empty;
            }

            // Allocate a new string and copy each input string into it
            var builder = new ValueStringBuilder(totalLength);
            int copiedLength = 0;
            for (int i = 0; i < values.Length; i++)
            {
                string? value = values[i];
                if (!string.IsNullOrEmpty(value))
                {
                    int valueLen = value!.Length;
                    if (valueLen > totalLength - copiedLength)
                    {
                        copiedLength = -1;
                        break;
                    }

                    builder.Append(value);
                    copiedLength += valueLen;
                }
            }

            // If we copied exactly the right amount, return the new string.  Otherwise,
            // something changed concurrently to mutate the input array: fall back to
            // doing the concatenation again, but this time with a defensive copy. This
            // fall back should be extremely rare.
            if (copiedLength == totalLength)
            {
                return builder.ToString();
            }
            else
            {
                builder.Dispose();
                return Concat((ReadOnlySpan<string?>)values.ToArray());
            }
        }

        /// <summary>
        /// Concatenates a span of strings, using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The character to use as a separator. <paramref name="separator"/> is included in the returned string only if <paramref name="value"/> has more than one element.</param>
        /// <param name="value">A span that contains the elements to concatenate.</param>
        /// <returns>
        /// A string that consists of the elements of <paramref name="value"/> delimited by the <paramref name="separator"/> string.
        /// -or-
        /// <see cref="string.Empty"/> if <paramref name="value"/> has zero elements.
        /// </returns>
        public static string Join(char separator, params ReadOnlySpan<string?> value)
        {
#if NET
            return JoinCore(new ReadOnlySpan<char>(in separator), value);
#else
            Span<char> buffer = stackalloc char[1];
            buffer[0] = separator;
            return JoinCore(buffer, value);
#endif
        }

        /// <summary>
        /// Concatenates the string representations of a span of objects, using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The character to use as a separator. <paramref name="separator"/> is included in the returned string only if value has more than one element.</param>
        /// <param name="values">A span of objects whose string representations will be concatenated.</param>
        /// <returns>
        /// A string that consists of the elements of <paramref name="values"/> delimited by the <paramref name="separator"/> character.
        /// -or-
        /// <see cref="string.Empty"/> if <paramref name="values"/> has zero elements.
        /// </returns>
        public static string Join(char separator, params ReadOnlySpan<object?> values)
        {
#if NET
            return JoinCore(new ReadOnlySpan<char>(in separator), values);
#else
            Span<char> buffer = stackalloc char[1];
            buffer[0] = separator;
            return JoinCore(buffer, values);
#endif
        }

        /// <summary>
        /// Concatenates a span of strings, using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the returned string only if <paramref name="value"/> has more than one element.</param>
        /// <param name="value">A span that contains the elements to concatenate.</param>
        /// <returns>
        /// A string that consists of the elements of <paramref name="value"/> delimited by the <paramref name="separator"/> string.
        /// -or-
        /// <see cref="string.Empty"/> if <paramref name="value"/> has zero elements.
        /// </returns>
        public static string Join(string? separator, params ReadOnlySpan<string?> value)
        {
            return JoinCore(separator.AsSpan(), value);
        }

        /// <summary>
        /// Concatenates the string representations of a span of objects, using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">A span of objects whose string representations will be concatenated.</param>
        /// <returns>
        /// A string that consists of the elements of <paramref name="values"/> delimited by the <paramref name="separator"/> string.
        /// -or-
        /// <see cref="string.Empty"/> if <paramref name="values"/> has zero elements.
        /// </returns>
        public static string Join(string? separator, params ReadOnlySpan<object?> values) =>
            JoinCore(separator.AsSpan(), values);

        private static string JoinCore(ReadOnlySpan<char> separator, ReadOnlySpan<object?> values)
        {
            if (values.IsEmpty)
            {
                return string.Empty;
            }

            string? firstString = values[0]?.ToString();

            if (values.Length == 1)
            {
                return firstString ?? string.Empty;
            }

            var result = new ValueStringBuilder(stackalloc char[StackallocCharBufferSizeLimit]);

            result.Append(firstString);

            for (int i = 1; i < values.Length; i++)
            {
                result.Append(separator);
                object? value = values[i];
                if (value != null)
                {
                    result.Append(value.ToString());
                }
            }

            return result.ToString();
        }

        private static string JoinCore(ReadOnlySpan<char> separator, ReadOnlySpan<string?> values)
        {
            if (values.Length <= 1)
            {
                return values.IsEmpty ?
                    string.Empty :
                    values[0] ?? string.Empty;
            }

            long totalSeparatorsLength = (long)(values.Length - 1) * separator.Length;
            if (totalSeparatorsLength > int.MaxValue)
            {
                ThrowHelper.ThrowOutOfMemoryException_StringTooLong();
            }
            int totalLength = (int)totalSeparatorsLength;

            // Calculate the length of the resultant string so we know how much space to allocate.
            foreach (string? value in values)
            {
                if (value != null)
                {
                    totalLength += value.Length;
                    if (totalLength < 0) // Check for overflow
                    {
                        ThrowHelper.ThrowOutOfMemoryException_StringTooLong();
                    }
                }
            }

            if (totalLength == 0)
            {
                return string.Empty;
            }

            // Copy each of the strings into the result buffer, interleaving with the separator.
            var builder = new ValueStringBuilder(totalLength);
            int copiedLength = 0;

            for (int i = 0; i < values.Length; i++)
            {
                // It's possible that another thread may have mutated the input array
                // such that our second read of an index will not be the same string
                // we got during the first read.

                // We range check again to avoid buffer overflows if this happens.

                if (values[i] is string value)
                {
                    int valueLen = value.Length;
                    if (valueLen > totalLength - copiedLength)
                    {
                        copiedLength = -1;
                        break;
                    }

                    // Fill in the value.
                    builder.Append(value);
                    copiedLength += valueLen;
                }

                if (i < values.Length - 1)
                {
                    // Fill in the separator.
                    // Special-case length 1 to avoid additional overheads of CopyTo.
                    // This is common due to the char separator overload.

                    if (separator.Length == 1)
                    {
                        builder.Append(separator[0]);
                    }
                    else
                    {
                        builder.Append(separator);
                    }

                    copiedLength += separator.Length;
                }
            }

            // If we copied exactly the right amount, return the new string.  Otherwise,
            // something changed concurrently to mutate the input array: fall back to
            // doing the concatenation again, but this time with a defensive copy. This
            // fall back should be extremely rare.
            if (copiedLength == totalLength)
            {

                return builder.ToString();
            }
            else
            {
                builder.Dispose();
                return JoinCore(separator, values.ToArray().AsSpan());
            }
        }

#endif
        }

#if !(NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
    /// <summary>
    /// Determines whether this string instance contains a specified character.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <param name="value">The character to seek.</param>
    /// <returns><see langword="true"/> if the value parameter occurs within this string; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    public static bool Contains(this string str, char value)
    {
        ArgumentNullException.ThrowIfNull(str);

        return str.IndexOf(value) >= 0;
    }

    /// <summary>
    /// Determines whether the specified character occurs within the string using the specified comparison rules.
    /// </summary>
    /// <param name="str">The string to search.</param>
    /// <param name="value">The character to seek.</param>
    /// <param name="comparisonType">One of the values that specifies the rules for the search.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> occurs within <paramref name="str"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="str"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="comparisonType"/> is not a valid <see cref="StringComparison"/> value.
    /// </exception>
    public static bool Contains(this string str, char value, StringComparison comparisonType)
    {
        ArgumentNullException.ThrowIfNull(str);

        return str.IndexOf(value, comparisonType) >= 0;
    }

    /// <summary>
    /// Determines whether the specified string occurs within this string using the specified comparison rules.
    /// </summary>
    /// <param name="str">The string to search.</param>
    /// <param name="value">The string to seek.</param>
    /// <param name="comparisonType">One of the values that specifies the rules for the search.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> occurs within <paramref name="str"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="str"/> or <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="comparisonType"/> is not a valid <see cref="StringComparison"/> value.
    /// </exception>
    public static bool Contains(this string str, string value, StringComparison comparisonType)
    {
        ArgumentNullException.ThrowIfNull(str);

        return str.IndexOf(value, comparisonType) >= 0;
    }

    /// <summary>
    /// Determines whether the end of this string instance matches a specified character.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <param name="value">The character to compare to the end of this string.</param>
    /// <returns><see langword="true"/> if value matches the end of this string; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    public static bool EndsWith(this string str, char value)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length == 0)
            return false;

        return str[str.Length - 1] == value;
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified Unicode character in this string instance.
    /// The search starts at a specified character position and examines a specified number of character positions.
    /// </summary>
    /// <param name="str">The string to search.</param>
    /// <param name="value">The character to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <param name="count">The number of character positions to examine.</param>
    /// <returns>The zero-based index position of value if that character is found, or -1 if it is not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when startIndex or count is less than 0, or startIndex plus count indicates a position not within this instance.
    /// </exception>
    public static int IndexOf(this string str, char value, int startIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(str);

        return str.IndexOf(value.ToString(), startIndex, count, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified character using the specified comparison rules.
    /// </summary>
    /// <param name="str">The string to search.</param>
    /// <param name="value">The character to seek.</param>
    /// <param name="comparisonType">One of the values that specifies the rules for the search.</param>
    /// <returns>
    /// The zero-based index of <paramref name="value"/> if found; otherwise, -1.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="str"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="comparisonType"/> is not a valid <see cref="StringComparison"/> value.
    /// </exception>
    public static int IndexOf(this string str, char value, StringComparison comparisonType)
    {
        ArgumentNullException.ThrowIfNull(str);

        return str.IndexOf(value.ToString(), comparisonType);
    }

    /// <summary>
    /// Reports the zero-based index position of the last occurrence of a specified Unicode character in this string instance.
    /// The search starts at a specified character position and proceeds backward for a specified number of character positions.
    /// </summary>
    /// <param name="str">The string to search.</param>
    /// <param name="value">The character to seek.</param>
    /// <param name="startIndex">
    /// The starting position of the search. The search proceeds from startIndex toward the beginning of this instance.
    /// </param>
    /// <param name="count">The number of character positions to examine.</param>
    /// <returns>The zero-based index position of value if that character is found, or -1 if it is not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when startIndex or count is less than 0, or startIndex minus count indicates a position not within this instance.
    /// </exception>
    public static int LastIndexOf(this string str, char value, int startIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(str);

        return str.LastIndexOf(value.ToString(), startIndex, count, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the beginning of this string instance matches a specified character.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <param name="value">The character to compare to the start of this string.</param>
    /// <returns><see langword="true"/> if value matches the beginning of this string; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    public static bool StartsWith(this string str, char value)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length == 0)
            return false;

        return str[0] == value;
    }
#endif

    /// <summary>
    /// Capitalizes the first letter of the given string using the specified culture.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="culture">The culture to use for case conversion. If null, the current culture is used.</param>
    /// <returns>The string with the first letter capitalized.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    public static string CapitalizeFirstLetter(this string str, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length == 0) return str;
        culture ??= CultureInfo.CurrentCulture;
        return char.ToUpper(str[0], culture) +
#if NETFRAMEWORK || NETSTANDARD2_0
            str.Substring(1);
#else
            str[1..];
#endif
    }

    /// <summary>
    /// Removes all whitespace characters from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>The string without any whitespace characters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    public static string RemoveWhiteSpace(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        var result = new ValueStringBuilder(str.Length);
        foreach(char c in str)
        {
            if (char.IsWhiteSpace(c)) continue;
            result.Append(c);
        }
        return result.ToString();
    }

    /// <summary>
    /// Converts the string to title case using the specified culture.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <param name="culture">
    /// The culture to use, or <see langword="null"/> to use the current culture.
    /// </param>
    /// <returns>
    /// The string converted to lowercase and then to title case.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="str"/> is <see langword="null"/>.
    /// </exception>
    public static string ToTitleCase(this string str, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length == 0) return str;
        culture ??= CultureInfo.CurrentCulture;
        return culture.TextInfo.ToTitleCase(str.ToLower(culture));
    }
}