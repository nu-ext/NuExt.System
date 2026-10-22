using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.IO;

/// <summary>
/// The <c>PathUtilities</c> class provides utility methods for performing common path operations.
/// </summary>
/// <remarks>
///
/// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.
/// See LICENSE file in the project root for full license information.
/// Original source code can be found at https://github.com/dotnet/runtime.
/// </remarks>
[SuppressMessage("Style", "IDE0056:Use index operator", Justification = "<Pending>")]
[SuppressMessage("Style", "IDE0057:Use range operator", Justification = "<Pending>")]
public static partial class PathUtilities
{
    // We consider '/' a directory separator on Unix like systems. 
    // On Windows both / and \ are equally accepted.

    /// <summary>
    /// Returns <see langword="true"/> if the two paths have the same root
    /// </summary>
    public static bool AreRootsEqual(scoped ReadOnlySpan<char> first, scoped ReadOnlySpan<char> second, bool isUnixLike)
    {
        int firstRootLength = GetRootLength(first, isUnixLike);
        int secondRootLength = GetRootLength(second, isUnixLike);

        if (firstRootLength != secondRootLength)
        {
            return false;
        }

        return first.Slice(0, firstRootLength).CompareTo(second.Slice(0, secondRootLength), isUnixLike ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) == 0;
    }

    /// <summary>
    /// Gets the count of common characters from the left optionally ignoring case
    /// </summary>
    internal static unsafe int EqualStartingCharacterCount(scoped ReadOnlySpan<char> first, scoped ReadOnlySpan<char> second, bool ignoreCase)
    {
        if (first.IsEmpty || second.IsEmpty) return 0;

        int commonChars = 0;

        fixed (char* f = first)
        fixed (char* s = second)
        {
            char* l = f;
            char* r = s;
            char* leftEnd = l + first.Length;
            char* rightEnd = r + second.Length;

            while (l != leftEnd && r != rightEnd
                                && (*l == *r || (ignoreCase && char.ToUpperInvariant(*l) == char.ToUpperInvariant(*r))))
            {
                commonChars++;
                l++;
                r++;
            }
        }

        return commonChars;
    }

    /// <summary>
    /// Get the common path length from the start of the string.
    /// </summary>
    internal static int GetCommonPathLength(scoped ReadOnlySpan<char> first, scoped ReadOnlySpan<char> second, bool isUnixLike)
    {
        int commonChars = EqualStartingCharacterCount(first, second, ignoreCase: !isUnixLike);

        // If nothing matches
        if (commonChars == 0)
            return commonChars;

        // Or we're a full string and equal length or match to a separator
        if (commonChars == first.Length
            && (commonChars == second.Length || IsDirectorySeparator(second[commonChars], isUnixLike)))
            return commonChars;

        if (commonChars == second.Length && IsDirectorySeparator(first[commonChars], isUnixLike))
            return commonChars;

        // It's possible we matched somewhere in the middle of a segment e.g. C:\Foodie and C:\Foobar.
        while (commonChars > 0 && !IsDirectorySeparator(first[commonChars - 1], isUnixLike))
            commonChars--;

        return commonChars;
    }

    /// <summary>
    /// <see langword="true"/> if the given character is a directory separator.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDirectorySeparator(char c, bool isUnixLike)
    {
        return isUnixLike ? Unix.IsDirectorySeparator(c) : Windows.IsDirectorySeparator(c);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUnixLikePlatform(char directorySeparatorChar)
    {
        return directorySeparatorChar == Unix.DirectorySeparatorChar;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the path is effectively empty for the current OS.
    /// For unix, this is empty or null. For Windows, this is empty, null, or
    /// just spaces ((char)32).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEffectivelyEmpty(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        return isUnixLike ? Unix.IsEffectivelyEmpty(path) : Windows.IsEffectivelyEmpty(path);
    }

    /// <summary>
    /// Determines whether the current path represents a root directory.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRoot(ReadOnlySpan<char> path, bool isUnixLike)
    {
        return !path.IsEmpty && path.Length == GetRootLength(path, isUnixLike);
    }

    /// <summary>
    /// Returns the directory portion of a file path. The returned value is empty
    /// if the specified path is null, empty, or a root (such as "\", "C:", or
    /// "\\server\share").
    /// </summary>
    /// <remarks>
    /// This method will not normalize directory separators.
    /// </remarks>
    public static ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path, bool isUnixLike)
    {
        if (IsEffectivelyEmpty(path, isUnixLike))
        {
            return [];
        }

        int end = GetDirectoryNameOffset(path, isUnixLike);
        return end >= 0 ? path.Slice(0, end) : [];
    }

    internal static int GetDirectoryNameOffset(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        int rootLength = GetRootLength(path, isUnixLike);
        int end = path.Length;
        if (end <= rootLength)
            return -1;

        while (end > rootLength && !IsDirectorySeparator(path[--end], isUnixLike)) ;

        // Trim off any remaining separators (to deal with C:\foo\\bar)
        while (end > rootLength && IsDirectorySeparator(path[end - 1], isUnixLike))
            end--;

        return end;
    }

    /// <summary>
    /// Returns the extension of the given path.
    /// </summary>
    /// <remarks>
    /// The returned value is an empty ReadOnlySpan if the given path does not include an extension.
    /// </remarks>
    public static ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path, bool isUnixLike)
    {
        int end = path.Length;
        for (int i = end - 1; i >= 0; i--)
        {
            char c = path[i];
            if (c == '.')
            {
                return i != end - 1 ? path.Slice(i, end - i) : [];
            }
            if (IsDirectorySeparator(c, isUnixLike))
            {
                break;
            }
        }
        return [];
    }

    /// <summary>
    /// The returned ReadOnlySpan contains the characters of the path that follows the last separator in path.
    /// </summary>
    public static ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path, bool isUnixLike)
    {
        int root = GetPathRoot(path, isUnixLike).Length;

        // We don't want to cut off "C:\file.txt:stream" (i.e. should be "file.txt:stream")
        // but we *do* want "C:Foo" => "Foo". This necessitates checking for the root.

        int i = isUnixLike ?
            path.LastIndexOf(Unix.DirectorySeparatorChar) :
            path.LastIndexOfAny(Windows.DirectorySeparatorChar, Windows.AltDirectorySeparatorChar);

        int start = i < root ? root : i + 1;

        return path.Slice(start);
    }

    /// <summary>
    /// Returns the characters between the last separator and last (.) in the path.
    /// </summary>
    public static ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path, bool isUnixLike)
    {
        var fileName = GetFileName(path, isUnixLike);
        int lastPeriod = fileName.LastIndexOf('.');
        return lastPeriod < 0 ?
            fileName // No extension was found
            : fileName.Slice(0, lastPeriod);
    }

    /// <summary>
    /// Normalizes the specified path using the selected platform semantics.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <param name="isUnixLike">
    /// <see langword="true"/> to use Unix path semantics;
    /// otherwise, <see langword="false"/> to use Windows path semantics.
    /// </param>
    /// <returns>The normalized path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> path, bool isUnixLike)
    {
        return isUnixLike ? Unix.GetFullPath(path) : Windows.GetFullPath(path);
    }

    /// <summary>
    /// Returns an absolute path from a relative path and a fully qualified base path.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> path, ReadOnlySpan<char> basePath, bool isUnixLike)
    {
        return isUnixLike ? Unix.GetFullPath(path, basePath) : Windows.GetFullPath(path, basePath);
    }

    /// <summary>
    /// Returns an array containing the characters that are not allowed in file names.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char[] GetInvalidFileNameChars(bool isUnixLike)
    {
        return isUnixLike ? Unix.GetInvalidFileNameChars() : Windows.GetInvalidFileNameChars();
    }

    /// <summary>
    /// Returns an array containing the characters that are not allowed in path names.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char[] GetInvalidPathChars(bool isUnixLike)
    {
        return isUnixLike ? Unix.GetInvalidPathChars() : Windows.GetInvalidPathChars();
    }

    /// <summary>
    /// Returns the root portion of the given path. The resulting string
    /// consists of those rightmost characters of the path that constitute the
    /// root of the path. Possible patterns for the resulting string are: An
    /// empty string (a relative path on the current drive), "\" (an absolute
    /// path on the current drive), "X:" (a relative path on a given drive,
    /// where X is the drive letter), "X:\" (an absolute path on a given drive),
    /// and "\\server\share" (a UNC path for a given server and share name).
    /// The resulting string is null if path is null. If the path is empty or
    /// only contains whitespace characters an ArgumentException gets thrown.
    /// </summary>
    /// <remarks>
    /// This method will not normalize directory separators.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path, bool isUnixLike)
    {
        return isUnixLike ? Unix.GetPathRoot(path) : Windows.GetPathRoot(path);
    }

    public static int GetPathSegments(scoped ReadOnlySpan<char> path, ICollection<string>? segments, bool isUnixLike)
    {
        if (path.IsEmpty)
            return 0;

        int count = 0;
        var root = GetPathRoot(path, isUnixLike);
        if (!root.IsEmpty)
        {
            segments?.Add(root.ToString());
            count++;
        }

        int start = root.Length;
        for (int i = start; i <= path.Length; i++)
        {
            if (i == path.Length || IsDirectorySeparator(path[i], isUnixLike))
            {
                if (i > start)
                {
                    segments?.Add(path.Slice(start, i - start).ToString());
                    count++;
                }
                start = i + 1;
            }
        }

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetRootLength(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        return isUnixLike ? Unix.GetRootLength(path) : Windows.GetRootLength(path);
    }

    /// <summary>
    /// Tests if a path's file name includes a file extension. A trailing period
    /// is not considered an extension.
    /// </summary>
    public static bool HasExtension(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        int end = path.Length;
        for (int i = end - 1; i >= 0; i--)
        {
            char c = path[i];
            if (c == '.')
            {
                return i != end - 1;
            }
            if (IsDirectorySeparator(c, isUnixLike))
            {
                break;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the path is fixed to a specific drive or UNC path. This method does no
    /// validation of the path (URIs will be returned as relative as a result).
    /// Returns false if the path specified is relative to the current drive or working directory.
    /// </summary>
    /// <remarks>
    /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
    /// assume that rooted paths <see cref="IsPathRooted(ReadOnlySpan{char}, bool)"/> are not relative. This isn't the case.
    /// "C:a" is drive relative-meaning that it will be resolved against the current directory
    /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
    /// will not be used to modify the path).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPathFullyQualified(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        return isUnixLike ? Unix.IsPathFullyQualified(path) : Windows.IsPathFullyQualified(path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPartiallyQualified(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        return isUnixLike ? Unix.IsPartiallyQualified(path) : Windows.IsPartiallyQualified(path);
    }

    // Tests if the given path contains a root. A path is considered rooted
    // if it starts with a backslash ("\") or a valid drive letter and a colon (":").
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPathRooted(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        return isUnixLike ? Unix.IsPathRooted(path) : Windows.IsPathRooted(path);
    }

    public static bool NormalizeDirectorySeparators(scoped ReadOnlySpan<char> path, ref ValueStringBuilder builder, bool isUnixLike)
    {
        return isUnixLike ? Unix.NormalizeDirectorySeparators(path, ref builder) : Windows.NormalizeDirectorySeparators(path, ref builder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PathEquals(scoped ReadOnlySpan<char> path1, scoped ReadOnlySpan<char> path2, bool isUnixLike)
    {
        return isUnixLike ? Unix.PathEquals(path1, path2) : Windows.PathEquals(path1, path2);
    }

    /// <summary>
    /// Creates a relative path from one path to another after normalizing both paths.
    /// Path comparison uses ordinal semantics for Unix paths and ordinal case-insensitive semantics for Windows paths.
    /// </summary>
    /// <param name="relativeTo">
    /// The source path the output should be relative to. This path is always considered to be a directory.
    /// </param>
    /// <param name="path">The destination path.</param>
    /// <param name="isUnixLike">
    /// <see langword="true"/> to use Unix path semantics;
    /// otherwise, <see langword="false"/> to use Windows path semantics.
    /// </param>
    /// <param name="basePath">
    /// The fully qualified base path used to resolve relative paths, or an empty span to normalize
    /// <paramref name="relativeTo"/> and <paramref name="path"/> without resolving them against a base path.
    /// </param>
    /// <returns>
    /// The relative path, or <paramref name="path"/> after normalization if the paths do not share the same root.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativeTo"/> or <paramref name="path"/> is empty, or
    /// <paramref name="basePath"/> is not fully qualified.
    /// </exception>
    public static ReadOnlySpan<char> GetRelativePath(scoped ReadOnlySpan<char> relativeTo, ReadOnlySpan<char> path, bool isUnixLike,
        ReadOnlySpan<char> basePath = default)
    {
        if (IsEffectivelyEmpty(relativeTo, isUnixLike))
            throw new ArgumentException(SR.Arg_PathEmpty, nameof(relativeTo));
        if (IsEffectivelyEmpty(path, isUnixLike))
            throw new ArgumentException(SR.Arg_PathEmpty, nameof(path));

        if (basePath.IsEmpty)
        {
            relativeTo = GetFullPath(relativeTo, isUnixLike);
            path = GetFullPath(path, isUnixLike);
        }
        else
        {
            relativeTo = GetFullPath(relativeTo, basePath, isUnixLike);
            path = GetFullPath(path, basePath, isUnixLike);
        }

        // Need to check if the roots are different- if they are we need to return the "to" path.
        if (!AreRootsEqual(relativeTo, path, isUnixLike))
            return path;

        int commonLength = GetCommonPathLength(relativeTo, path, isUnixLike);

        // If there is nothing in common they can't share the same root, return the "to" path as is.
        if (commonLength == 0)
            return path;

        // Trailing separators aren't significant for comparison
        int relativeToLength = relativeTo.Length;
        if (EndsInDirectorySeparator(relativeTo, isUnixLike))
            relativeToLength--;

        bool pathEndsInSeparator = EndsInDirectorySeparator(path, isUnixLike);
        int pathLength = path.Length;
        if (pathEndsInSeparator)
            pathLength--;

        // If we have effectively the same path, return "."
        if (relativeToLength == pathLength && commonLength >= relativeToLength) return ".".AsSpan();

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

        var directorySeparatorChar = isUnixLike ? Unix.DirectorySeparatorChar : Windows.DirectorySeparatorChar;

        // Add parent segments for segments past the common on the "from" path
        if (commonLength < relativeToLength)
        {
            sb.Append("..");

            for (int i = commonLength + 1; i < relativeToLength; i++)
            {
                if (IsDirectorySeparator(relativeTo[i], isUnixLike))
                {
                    sb.Append(directorySeparatorChar);
                    sb.Append("..");
                }
            }
        }
        else if (IsDirectorySeparator(path[commonLength], isUnixLike))
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
                sb.Append(directorySeparatorChar);
            }

            sb.Append(path.Slice(commonLength, differenceLength));
        }

        return sb.ToString().AsSpan();
    }

    /// <summary>
    /// Try to remove relative segments from the given path (without combining with a root).
    /// </summary>
    /// <param name="path">Input path</param>
    /// <param name="rootLength">The length of the root of the given path</param>
    /// <param name="isUnixLike"></param>
    internal static ReadOnlySpan<char> RemoveRelativeSegments(ReadOnlySpan<char> path, int rootLength, bool isUnixLike)
    {
        var sb = new ValueStringBuilder(stackalloc char[260 /* PathInternal.MaxShortPath */]);

        if (RemoveRelativeSegments(path, rootLength, ref sb, isUnixLike))
        {
            return sb.ToString().AsSpan();
        }

        sb.Dispose();
        return path;
    }

    /// <summary>
    /// Try to remove relative segments from the given path (without combining with a root).
    /// </summary>
    /// <param name="path">Input path</param>
    /// <param name="rootLength">The length of the root of the given path</param>
    /// <param name="sb">String builder that will store the result</param>
    /// <param name="isUnixLike"></param>
    /// <returns>"true" if the path was modified</returns>
    internal static bool RemoveRelativeSegments(scoped ReadOnlySpan<char> path, int rootLength, ref ValueStringBuilder sb, bool isUnixLike)
    {
        //Debug.Assert(rootLength > 0);
        bool flippedSeparator = false;

        int skip = rootLength;
        // We treat "\.." , "\." and "\\" as a relative segment. We want to collapse the first separator past the root presuming
        // the root actually ends in a separator. Otherwise the first segment for RemoveRelativeSegments
        // in cases like "\\?\C:\.\" and "\\?\C:\..\", the first segment after the root will be ".\" and "..\" which is not considered as a relative segment and hence not be removed.
        if (skip > 0 && IsDirectorySeparator(path[skip - 1], isUnixLike))
            skip--;

        // Remove "//", "/./", and "/../" from the path by copying each character to the output,
        // except the ones we're removing, such that the builder contains the normalized path
        // at the end.
        if (skip > 0)
        {
            sb.Append(path.Slice(0, skip));
        }

        for (int i = skip; i < path.Length; i++)
        {
            char c = path[i];

            if (IsDirectorySeparator(c, isUnixLike) && i + 1 < path.Length)
            {
                // Skip this character if it's a directory separator and if the next character is, too,
                // e.g. "parent//child" => "parent/child"
                if (IsDirectorySeparator(path[i + 1], isUnixLike))
                {
                    continue;
                }

                // Skip this character and the next if it's referring to the current directory,
                // e.g. "parent/./child" => "parent/child"
                if ((i + 2 == path.Length || IsDirectorySeparator(path[i + 2], isUnixLike)) &&
                    path[i + 1] == '.')
                {
                    i++;
                    continue;
                }

                // Skip this character and the next two if it's referring to the parent directory,
                // e.g. "parent/child/../grandchild" => "parent/grandchild"
                if (i + 2 < path.Length &&
                    (i + 3 == path.Length || IsDirectorySeparator(path[i + 3], isUnixLike)) &&
                    path[i + 1] == '.' && path[i + 2] == '.')
                {
                    // Unwind back to the last slash (and if there isn't one, clear out everything).
                    int s;
                    for (s = sb.Length - 1; s >= skip; s--)
                    {
                        if (IsDirectorySeparator(sb[s], isUnixLike))
                        {
                            sb.Length = (i + 3 >= path.Length && s == skip) ? s + 1 : s; // to avoid removing the complete "\tmp\" segment in cases like \\?\C:\tmp\..\, C:\tmp\..
                            break;
                        }
                    }
                    if (s < skip)
                    {
                        sb.Length = skip;
                    }

                    i += 2;
                    continue;
                }
            }

            // Normalize the directory separator if needed
            if (!isUnixLike && c != Windows.DirectorySeparatorChar && c == Windows.AltDirectorySeparatorChar)
            {
                c = Windows.DirectorySeparatorChar;
                flippedSeparator = true;
            }

            sb.Append(c);
        }

        // If we haven't changed the source path, return the original
        if (!flippedSeparator && sb.Length == path.Length)
        {
            return false;
        }

        // We may have eaten the trailing separator from the root when we started and not replaced it
        if (skip != rootLength && sb.Length < rootLength)
        {
            sb.Append(path[rootLength - 1]);
        }

        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the path starts in a directory separator.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithDirectorySeparator(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        return path.Length > 0 && IsDirectorySeparator(path[0], isUnixLike);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the path ends in a directory separator.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsInDirectorySeparator(scoped ReadOnlySpan<char> path, bool isUnixLike)
    {
        return path.Length > 0 && IsDirectorySeparator(path[path.Length - 1], isUnixLike);
    }

    /// <summary>
    /// Trims one trailing directory separator beyond the root of the path.
    /// </summary>
    public static ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path, bool isUnixLike)
    {
        return EndsInDirectorySeparator(path, isUnixLike) && !IsRoot(path, isUnixLike) ?
            path.Slice(0, path.Length - 1) :
            path;
    }

    /// <summary>
    /// Removes trailing directory separator characters.
    /// </summary>
    private static ReadOnlySpan<char> TrimTrailingSeparators(ReadOnlySpan<char> path, bool isUnixLike)
    {
        int rootLength = GetRootLength(path, isUnixLike);
        int end = path.Length;

        while (end > rootLength && IsDirectorySeparator(path[end - 1], isUnixLike))
        {
            end--;
        }

        return path.Slice(0, end);
    }

    #region Utils

    /// <summary>Combines two strings into a path.</summary>
    /// <param name="path1">The first path to combine.</param>
    /// <param name="path2">The second path to combine.</param>
    /// <param name="isUnixLike"></param>
    /// <returns>The combined paths. If one of the specified paths is a zero-length string, this method returns the other path. If <paramref name="path2" /> contains an absolute path, this method returns <paramref name="path2" />.</returns>
    public static ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, bool isUnixLike)
    {
        return CombineInternal(path1, path2, isUnixLike);
    }

    /// <summary>Combines three strings into a path.</summary>
    /// <param name="path1">The first path to combine.</param>
    /// <param name="path2">The second path to combine.</param>
    /// <param name="path3">The third path to combine.</param>
    /// <param name="isUnixLike"></param>
    /// <returns>The combined paths.</returns>
    public static ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, bool isUnixLike)
    {
        return CombineInternal(path1, path2, path3, isUnixLike);
    }

    /// <summary>Combines four strings into a path.</summary>
    /// <param name="path1">The first path to combine.</param>
    /// <param name="path2">The second path to combine.</param>
    /// <param name="path3">The third path to combine.</param>
    /// <param name="path4">The fourth path to combine.</param>
    /// <param name="isUnixLike"></param>
    /// <returns>The combined paths.</returns>
    public static ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4, bool isUnixLike)
    {
        return CombineInternal(path1, path2, path3, path4, isUnixLike);
    }

    /// <summary>
    /// Combines a span of strings into a path.
    /// </summary>
    /// <param name="isUnixLike"></param>
    /// <param name="paths">A span of parts of the path.</param>
    /// <returns>The combined paths.</returns>
    public static ReadOnlySpan<char> Combine(bool isUnixLike, params ReadOnlySpan<string?> paths)
    {
        if (paths.IsEmpty)
        {
            return [];
        }

        int maxSize = 0;
        int firstComponent = 0;

        // We have two passes, the first calculates how large a buffer to allocate and does some precondition
        // checks on the paths passed in.  The second actually does the combination.

        for (int i = 0; i < paths.Length; i++)
        {
            string? path = paths[i];
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (IsPathRooted(path.AsSpan(), isUnixLike))
            {
                firstComponent = i;
                maxSize = path!.Length;
            }
            else
            {
                maxSize += path!.Length;
            }

            char ch = path[path.Length - 1];
            if (!IsDirectorySeparator(ch, isUnixLike))
                maxSize++;
        }

        var builder = new ValueStringBuilder(stackalloc char[260]); // MaxShortPath on Windows
        builder.EnsureCapacity(maxSize);

        var directorySeparatorChar = (isUnixLike ? Unix.DirectorySeparatorCharAsString : Windows.DirectorySeparatorCharAsString);

        for (int i = firstComponent; i < paths.Length; i++)
        {
            string? path = paths[i];
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (builder.Length == 0)
            {
                builder.Append(path);
            }
            else
            {
                char ch = builder[builder.Length - 1];
                if (!IsDirectorySeparator(ch, isUnixLike))
                {
                    builder.Append(directorySeparatorChar);
                }

                builder.Append(paths[i]);
            }
        }

        return builder.ToString().AsSpan();
    }

    private static ReadOnlySpan<char> CombineInternal(ReadOnlySpan<char> first, ReadOnlySpan<char> second, bool isUnixLike)
    {
        if (first.IsEmpty)
            return second;

        if (second.IsEmpty)
            return first;

        if (IsPathRooted(second, isUnixLike))
            return second;

        return JoinInternal(first, second, isUnixLike);
    }

    private static ReadOnlySpan<char> CombineInternal(ReadOnlySpan<char> first, ReadOnlySpan<char> second, ReadOnlySpan<char> third, bool isUnixLike)
    {
        if (first.IsEmpty)
            return CombineInternal(second, third, isUnixLike);
        if (second.IsEmpty)
            return CombineInternal(first, third, isUnixLike);
        if (third.IsEmpty)
            return CombineInternal(first, second, isUnixLike);

        if (IsPathRooted(third, isUnixLike))
            return third;
        if (IsPathRooted(second, isUnixLike))
            return CombineInternal(second, third, isUnixLike);

        return JoinInternal(first, second, third, isUnixLike);
    }

    private static ReadOnlySpan<char> CombineInternal(ReadOnlySpan<char> first, ReadOnlySpan<char> second, ReadOnlySpan<char> third, ReadOnlySpan<char> fourth, bool isUnixLike)
    {
        if (first.IsEmpty)
            return CombineInternal(second, third, fourth, isUnixLike);
        if (second.IsEmpty)
            return CombineInternal(first, third, fourth, isUnixLike);
        if (third.IsEmpty)
            return CombineInternal(first, second, fourth, isUnixLike);
        if (fourth.IsEmpty)
            return CombineInternal(first, second, third, isUnixLike);

        if (IsPathRooted(fourth, isUnixLike))
            return fourth;
        if (IsPathRooted(third, isUnixLike))
            return CombineInternal(third, fourth, isUnixLike);
        if (IsPathRooted(second, isUnixLike))
            return CombineInternal(second, third, fourth, isUnixLike);

        return JoinInternal(first, second, third, fourth, isUnixLike);
    }

    /// <summary>Concatenates two path components into a single path.</summary>
    /// <param name="path1">A character span that contains the first path to join.</param>
    /// <param name="path2">A character span that contains the second path to join.</param>
    /// <param name="isUnixLike"></param>
    /// <returns>The combined paths.</returns>
    /// <remarks>
    /// Unlike Combine(), Join() methods do not consider rooting. They simply combine paths, ensuring that there
    /// is a directory separator between them.
    /// </remarks>
    public static ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, bool isUnixLike)
    {
        if (path1.Length == 0)
            return path2;
        if (path2.Length == 0)
            return path1;

        return JoinInternal(path1, path2, isUnixLike);
    }

    /// <summary>Concatenates three path components into a single path.</summary>
    /// <param name="path1">A character span that contains the first path to join.</param>
    /// <param name="path2">A character span that contains the second path to join.</param>
    /// <param name="path3">A character span that contains the third path to join.</param>
    /// <param name="isUnixLike"></param>
    /// <returns>The concatenated path.</returns>
    /// <remarks>
    /// Unlike Combine(), Join() methods do not consider rooting. They simply combine paths, ensuring that there
    /// is a directory separator between them.
    /// </remarks>
    public static ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, bool isUnixLike)
    {
        if (path1.Length == 0)
            return Join(path2, path3, isUnixLike);

        if (path2.Length == 0)
            return Join(path1, path3, isUnixLike);

        if (path3.Length == 0)
            return Join(path1, path2, isUnixLike);

        return JoinInternal(path1, path2, path3, isUnixLike);
    }

    /// <summary>Concatenates four path components into a single path.</summary>
    /// <param name="path1">A character span that contains the first path to join.</param>
    /// <param name="path2">A character span that contains the second path to join.</param>
    /// <param name="path3">A character span that contains the third path to join.</param>
    /// <param name="path4">A character span that contains the fourth path to join.</param>
    /// <param name="isUnixLike"></param>
    /// <returns>The concatenated path.</returns>
    public static ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4, bool isUnixLike)
    {
        if (path1.Length == 0)
            return Join(path2, path3, path4, isUnixLike);

        if (path2.Length == 0)
            return Join(path1, path3, path4, isUnixLike);

        if (path3.Length == 0)
            return Join(path1, path2, path4, isUnixLike);

        if (path4.Length == 0)
            return Join(path1, path2, path3, isUnixLike);

        return JoinInternal(path1, path2, path3, path4, isUnixLike);
    }

    /// <summary>
    /// Concatenates a span of paths into a single path.
    /// </summary>
    /// <param name="isUnixLike"></param>
    /// <param name="paths">A span of paths.</param>
    /// <returns>The concatenated path.</returns>
    public static ReadOnlySpan<char> Join(bool isUnixLike, params ReadOnlySpan<string?> paths)
    {
        if (paths.IsEmpty)
        {
            return [];
        }

        int maxSize = 0;
        foreach (string? path in paths)
        {
            maxSize += path?.Length ?? 0;
        }
        maxSize += paths.Length - 1;

        var builder = new ValueStringBuilder(stackalloc char[260]); // MaxShortPath on Windows
        builder.EnsureCapacity(maxSize);

        var directorySeparatorChar = (isUnixLike ? Unix.DirectorySeparatorCharAsString : Windows.DirectorySeparatorCharAsString);

        for (int i = 0; i < paths.Length; i++)
        {
            string? path = paths[i];
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (builder.Length == 0)
            {
                builder.Append(path);
            }
            else
            {
                if (!IsDirectorySeparator(builder[builder.Length - 1], isUnixLike) && !IsDirectorySeparator(path![0], isUnixLike))
                {
                    builder.Append(directorySeparatorChar);
                }

                builder.Append(path);
            }
        }

        return builder.ToString().AsSpan();
    }

    private static ReadOnlySpan<char> JoinInternal(scoped ReadOnlySpan<char> first, scoped ReadOnlySpan<char> second, bool isUnixLike)
    {
        Debug.Assert(first.Length > 0 && second.Length > 0, "should have dealt with empty paths");

        bool hasSeparator = IsDirectorySeparator(first[first.Length - 1], isUnixLike) || IsDirectorySeparator(second[0], isUnixLike);

        var directorySeparatorCharAsSpan = (isUnixLike ? Unix.DirectorySeparatorCharAsString : Windows.DirectorySeparatorCharAsString).AsSpan();

        return hasSeparator ? string.Concat(first, second).AsSpan() : string.Concat(first, directorySeparatorCharAsSpan, second);
    }

    private static ReadOnlySpan<char> JoinInternal(scoped ReadOnlySpan<char> first, scoped ReadOnlySpan<char> second, scoped ReadOnlySpan<char> third, bool isUnixLike)
    {
        Debug.Assert(first.Length > 0 && second.Length > 0 && third.Length > 0, "should have dealt with empty paths");

        bool firstHasSeparator = IsDirectorySeparator(first[first.Length - 1], isUnixLike) || IsDirectorySeparator(second[0], isUnixLike);
        bool secondHasSeparator = IsDirectorySeparator(second[second.Length - 1], isUnixLike) || IsDirectorySeparator(third[0], isUnixLike);

        var directorySeparatorCharAsSpan = (isUnixLike ? Unix.DirectorySeparatorCharAsString : Windows.DirectorySeparatorCharAsString).AsSpan();

        return (firstHasSeparator, secondHasSeparator) switch
        {
            (false, false) => string.Concat(first, directorySeparatorCharAsSpan, second, directorySeparatorCharAsSpan, third),
            (false, true) => string.Concat(first, directorySeparatorCharAsSpan, second, third),
            (true, false) => string.Concat(first, second, directorySeparatorCharAsSpan, third),
            (true, true) => string.Concat(first, second, third),
        };
        //return JoinInternal(JoinInternal(first, second, isUnixLike), third, isUnixLike);
    }

    private static ReadOnlySpan<char> JoinInternal(ReadOnlySpan<char> first, ReadOnlySpan<char> second, ReadOnlySpan<char> third, ReadOnlySpan<char> fourth, bool isUnixLike)
    {
        Debug.Assert(first.Length > 0 && second.Length > 0 && third.Length > 0 && fourth.Length > 0, "should have dealt with empty paths");
        return JoinInternal(JoinInternal(first, second, third, isUnixLike), fourth, isUnixLike);
    }

    #endregion
}
