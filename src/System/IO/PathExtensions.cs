using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.IO;

public static class PathExtensions
{
    extension(Path)
    {
        /// <summary>Returns a comparison that can be used to compare file and directory names for equality.</summary>
        public static StringComparison StringComparison
        {
            get
            {
                return Path.IsCaseSensitivePlatform ?
                    StringComparison.Ordinal :
                    StringComparison.OrdinalIgnoreCase;
            }
        }

        /// <summary>
        /// Gets whether path comparisons should use case-sensitive semantics based on the platform default.
        /// This value does not detect case-sensitivity settings of a specific file system or directory.
        /// </summary>
        public static bool IsCaseSensitivePlatform
        {
            get
            {
#if NET5_0_OR_GREATER
                return !(OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() || OperatingSystem.IsTvOS());
#else
                return !(PlatformInformation.IsWindows || PlatformInformation.IsMacOS);
#endif
            }
        }

        /// <summary>Gets whether the system is Unix-like.</summary>
        public static bool IsUnixLike => PlatformInformation.IsUnix;

#if !(NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
        /// <summary>
        /// Create a relative path from one path to another. Paths will be resolved before calculating the difference.
        /// Default path comparison for the active platform will be used (OrdinalIgnoreCase for Windows or Mac, Ordinal for Unix).
        /// </summary>
        /// <param name="relativeTo">The source path the output should be relative to. This path is always considered to be a directory.</param>
        /// <param name="path">The destination path.</param>
        /// <returns>The relative path or <paramref name="path"/> if the paths don't share the same root.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="relativeTo"/> or <paramref name="path"/> is <see langword="null"/> or an empty string.</exception>
        public static string GetRelativePath(string relativeTo, string path)
        {
            return GetRelativePath(relativeTo, path, Path.StringComparison);
        }

        private static string GetRelativePath(string relativeTo, string path, StringComparison comparisonType)
        {
            ArgumentNullException.ThrowIfNull(relativeTo);
            ArgumentNullException.ThrowIfNull(path);

            if (PathUtilities.IsEffectivelyEmpty(relativeTo.AsSpan(), Path.IsUnixLike))
                throw new ArgumentException(SR.Arg_PathEmpty, nameof(relativeTo));
            if (PathUtilities.IsEffectivelyEmpty(path.AsSpan(), Path.IsUnixLike))
                throw new ArgumentException(SR.Arg_PathEmpty, nameof(path));

            Debug.Assert(comparisonType == StringComparison.Ordinal || comparisonType == StringComparison.OrdinalIgnoreCase);

            relativeTo = Path.GetFullPath(relativeTo);
            path = Path.GetFullPath(path);

            // Need to check if the roots are different- if they are we need to return the "to" path.
            if (!PathUtilities.AreRootsEqual(relativeTo, path, Path.IsUnixLike))
                return path;

            int commonLength = PathUtilities.GetCommonPathLength(relativeTo, path, Path.IsUnixLike);

            // If there is nothing in common they can't share the same root, return the "to" path as is.
            if (commonLength == 0)
                return path;

            // Trailing separators aren't significant for comparison
            int relativeToLength = relativeTo.Length;
            if (PathUtilities.EndsInDirectorySeparator(relativeTo.AsSpan(), Path.IsUnixLike))
                relativeToLength--;

            bool pathEndsInSeparator = PathUtilities.EndsInDirectorySeparator(path.AsSpan(), Path.IsUnixLike);
            int pathLength = path.Length;
            if (pathEndsInSeparator)
                pathLength--;

            // If we have effectively the same path, return "."
            if (relativeToLength == pathLength && commonLength >= relativeToLength) return ".";

            // We have the same root, we need to calculate the difference now using the
            // common Length and Segment count past the length.
            //
            // Some examples:
            //
            //  C:\Foo C:\Bar L3, S1 -> ..\Bar
            //  C:\Foo C:\Foo\Bar L6, S0 -> Bar
            //  C:\Foo\Bar C:\Bar\Bar L3, S2 -> ..\..\Bar\Bar
            //  C:\Foo\Foo C:\Foo\Bar L7, S1 -> ..\Bar

            var sb = new ValueStringBuilder(stackalloc char[260]);
            sb.EnsureCapacity(Math.Max(relativeTo.Length, path.Length));

            // Add parent segments for segments past the common on the "from" path
            if (commonLength < relativeToLength)
            {
                sb.Append("..");

                for (int i = commonLength + 1; i < relativeToLength; i++)
                {
                    if (PathUtilities.IsDirectorySeparator(relativeTo[i], Path.IsUnixLike))
                    {
                        sb.Append(Path.DirectorySeparatorChar);
                        sb.Append("..");
                    }
                }
            }
            else if (PathUtilities.IsDirectorySeparator(path[commonLength], Path.IsUnixLike))
            {
                // No parent segments and we need to eat the initial separator
                //  (C:\Foo C:\Foo\Bar case)
                commonLength++;
            }

            // Now add the rest of the "to" path, adding back the trailing separator
            int differenceLength = pathLength - commonLength;
            if (pathEndsInSeparator)
                differenceLength++;

            if (differenceLength > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(Path.DirectorySeparatorChar);
                }

                sb.Append(path.AsSpan(commonLength, differenceLength));
            }

            return sb.ToString();
        }
#endif
    }
}
