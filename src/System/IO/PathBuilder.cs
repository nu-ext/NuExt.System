using System.Collections.Generic;

namespace System.IO;

/// <summary>
/// Provides a mutable builder for constructing paths.
/// </summary>
public partial class PathBuilder : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathBuilder"/> class.
    /// </summary>
    public partial PathBuilder();

    /// <summary>
    /// Initializes a new instance of the <see cref="PathBuilder"/> class with a specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The initial number of characters the builder can hold.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialCapacity"/> is less than 0.</exception>
    public partial PathBuilder(int initialCapacity);

    /// <summary>
    /// Initializes a new instance of the <see cref="PathBuilder"/> class with an initial path.
    /// </summary>
    /// <param name="initialPath">The initial path to start the builder with.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialPath"/> is null.</exception>
    public partial PathBuilder(IEnumerable<char> initialPath);

    /// <summary>
    /// Gets the capacity of the builder.
    /// </summary>
    public partial int Capacity { get; }

    /// <summary>
    /// Gets or sets the directory separator character.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is neither <c>'/'</c> nor <c>'\'</c>.
    /// </exception>
    public partial char DirectorySeparatorChar { get; set; }

    /// <summary>
    /// Gets or sets the length of the path.
    /// </summary>
    public partial int Length { get; set; }

    /// <summary>
    /// Gets a reference to the character at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the character to get.</param>
    /// <returns>A reference to the character at the specified index.</returns>
    public partial ref char this[int index] { get; }

    /// <summary>
    /// Appends a character to the path without introducing a directory separator.
    /// </summary>
    /// <param name="c">The character to append.</param>
    public partial void Add(char c);

    /// <summary>
    /// Adds a string segment to the path without adding a directory separator.
    /// </summary>
    /// <param name="s">The string segment to append.</param>
    public partial void Add(scoped ReadOnlySpan<char> s);

    /// <summary>
    /// Appends a string to the path without introducing a directory separator.
    /// </summary>
    /// <param name="s">The string element to append.</param>
    public partial void Add(string? s);

    /// <summary>
    /// Appends elements to the path with a directory separator specified by <see cref="DirectorySeparatorChar"/>.
    /// </summary>
    /// <param name="path">The path element to append.</param>
    public partial void Append(scoped ReadOnlySpan<char> path);

    /// <summary>
    /// Appends elements to the path with a directory separator specified by <see cref="DirectorySeparatorChar"/>.
    /// </summary>
    /// <param name="path">The path element to append.</param>
    public partial void Append(string? path);

    /// <summary>
    /// Returns a read-only span around the contents of the builder.
    /// </summary>
    /// <returns>A read-only span that represents the current contents of the builder.</returns>
    public partial ReadOnlySpan<char> AsSpan();

    /// <summary>
    /// Returns a read-only span around the contents of the builder, optionally ensuring a null character after <see cref="Length"/>.
    /// </summary>
    /// <param name="terminate">True to ensure a null character after <see cref="Length"/>; otherwise, <see langword="false"/>.</param>
    /// <returns>A read-only span that represents the current contents of the builder.</returns>
    public partial ReadOnlySpan<char> AsSpan(bool terminate);

    /// <summary>
    /// Returns a read-only span around the contents of the builder starting at a specified position.
    /// </summary>
    /// <param name="start">The zero-based index from which to begin the span.</param>
    /// <returns>A read-only span that represents the current contents of the builder starting at <paramref name="start"/>.</returns>
    public partial ReadOnlySpan<char> AsSpan(int start);

    /// <summary>
    /// Returns a read-only span around the contents of the builder with a specified starting position and length.
    /// </summary>
    /// <param name="start">The zero-based index from which to begin the span.</param>
    /// <param name="length">The number of characters to include in the span.</param>
    /// <returns>A read-only span that represents the current contents of the builder starting at <paramref name="start"/> and including <paramref name="length"/> characters.</returns>
    public partial ReadOnlySpan<char> AsSpan(int start, int length);

    /// <summary>
    /// Changes the extension of a path.
    /// </summary>
    /// <param name="extension">The new extension (with or without a leading period). Specify <see langword="null" /> to remove an existing extension from the path.</param>
    /// <returns><see langword="true" /> if the extension was successfully changed; otherwise, <see langword="false" />.</returns>
    public partial bool ChangeExtension(string? extension);

    /// <summary>
    ///  Changes the file name portion of the path.
    /// </summary>
    /// <param name="newFileName">The new file name.</param>
    /// <returns><see langword="true" /> if the file name was successfully changed or removed; otherwise, <see langword="false" />.</returns>
    public partial bool ChangeFileName(scoped ReadOnlySpan<char> newFileName);

    /// <summary>
    /// Changes the file name portion of the path.
    /// </summary>
    /// <param name="newFileName">The new file name. Specify <see langword="null" /> to remove an existing file name from the path.</param>
    /// <returns><see langword="true" /> if the file name was successfully changed or removed; otherwise, <see langword="false" />.</returns>
    public partial bool ChangeFileName(string? newFileName);

    /// <summary>
    /// Clears the path.
    /// </summary>
    public partial void Clear();

    /// <summary>
    /// Releases any resources used by the <see cref="PathBuilder"/> instance.
    /// </summary>
    public partial void Dispose();

    /// <summary>
    /// Returns <see langword="true"/> if the path ends in a directory separator.
    /// </summary>
    /// <returns><see langword="true" /> if the path ends in a directory separator; otherwise, <see langword="false" />.</returns>
    public partial bool EndsInDirectorySeparator();

    /// <summary>
    /// Ensures that the path ends with a directory separator character.
    /// </summary>
    /// <remarks>
    /// This method modifies the current instance of the path by appending a trailing 
    /// directory separator specified by <see cref="DirectorySeparatorChar"/> if it is not already present.
    /// </remarks>
    /// <returns><see langword="true" /> if a trailing directory separator was added; otherwise, <see langword="false" />.</returns>
    public partial bool EnsureTrailingSeparator();

    /// <summary>
    /// Ensures that the capacity of this builder is at least the specified value.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    public partial void EnsureCapacity(int capacity);

    /// <summary>Returns the directory information for the path represented by a character span.</summary>
    /// <returns>Directory information for path, or an empty span if path is empty, or a root (such as \, C:, or \\server\share).</returns>
    /// <remarks>This method will not normalize directory separators.</remarks>
    public partial ReadOnlySpan<char> GetDirectoryName();

    /// <summary>
    /// Returns the extension of the given path.
    /// </summary>
    /// <remarks>The returned value is an empty <see cref="ReadOnlySpan{T}"/> if the given path does not include an extension.</remarks>
    /// <returns>A read-only span that represents the extension of the path.</returns>
    public partial ReadOnlySpan<char> GetExtension();

    /// <summary>
    /// Returns the file name and extension of a file path that is represented by a read-only character span.
    /// </summary>
    /// <returns>The characters after the last directory separator character in the path.</returns>
    public partial ReadOnlySpan<char> GetFileName();

    /// <summary>
    /// Returns the file name of the specified path string without the extension.
    /// </summary>
    /// <returns>The characters between the last separator and last (.) in the path.</returns>
    public partial ReadOnlySpan<char> GetFileNameWithoutExtension();

    /// <summary>
    /// Expands the path to a fully qualified path.
    /// </summary>
    /// <returns>The expanded path.</returns>
    public partial ReadOnlySpan<char> GetFullPath();

    /// <summary>
    /// Returns an absolute path from a relative path and a fully qualified base path.
    /// </summary>
    /// <param name="basePath">The beginning of a fully qualified path.</param>
    /// <exception cref="T:System.ArgumentException">
    ///         <paramref name="basePath" /> is not a fully qualified path.
    /// 
    /// -or-
    /// 
    /// The path or <paramref name="basePath" /> contains one or more of the invalid characters.</exception>
    /// <returns>The absolute path.</returns>
    public partial ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> basePath);

    /// <summary>
    /// Gets the root directory information from the path.
    /// </summary>
    /// <remarks>
    /// This method extracts the root directory portion of the specified path. 
    /// On Windows, this would typically include the drive letter and the volume separator (e.g., "C:\").
    /// On Unix-based systems, it would generally return "/" for absolute paths.
    /// If the path does not contain any root directory information, an empty span is returned.
    /// </remarks>
    /// <returns>The characters containing the root directory of the path.</returns>
    public partial ReadOnlySpan<char> GetPathRoot();

    /// <summary>
    /// Splits the specified path into its individual segments (such as directories and the file name)
    /// and adds them to the provided collection, if not null.
    /// </summary>
    /// <param name="segments">A collection to which each segment of the path will be added, or null.</param>
    /// <remarks>
    /// This method separates the path into its constituent parts based on the operating system's path separator.
    /// - On Windows, segments are separated by either '\' or '/' characters.
    /// - On Unix-based systems, segments are separated by '/' characters.
    /// The root directory, if present, is included as the first segment.
    /// If <paramref name="segments"/> is null, no segments will be added but the method will still return the count 
    /// of path segments.
    /// </remarks>
    /// <returns>The number of segments in the path.</returns>
    public partial int GetPathSegments(ICollection<string>? segments);

    /// <summary>
    /// Returns a relative path from the path to another.
    /// </summary>
    /// <param name="path">The destination path.</param>
    /// <param name="basePath">The beginning of a fully qualified path or empty.</param>
    /// <exception cref="T:System.ArgumentException">
    /// The path or <paramref name="path" /> is effectively empty.</exception>
    /// <returns>The relative path, or <paramref name="path" /> if the paths don't share the same root.</returns>
    public partial ReadOnlySpan<char> GetRelativePath(ReadOnlySpan<char> path, ReadOnlySpan<char> basePath = default);

    /// <summary>
    /// Tests if the path's file name includes a file extension. A trailing period is not considered an extension.
    /// </summary>
    /// <returns><see langword="true" /> if the path has an extension; otherwise, <see langword="false" />.</returns>
    public partial bool HasExtension();

    /// <summary>
    /// Returns <see langword="true"/> if the path is effectively empty for the current OS.
    /// </summary>
    /// <returns><see langword="true" /> if the path effectively empty; otherwise, <see langword="false" />.</returns>
    /// <remarks>
    /// For unix, this is empty or null. For Windows, this is empty, null, or just spaces ((char)32).
    /// </remarks>
    public partial bool IsEffectivelyEmpty();

    /// <summary>
    /// Returns a value that indicates whether the path is fixed to a specific drive or UNC path.
    /// </summary>
    /// <returns><see langword="true" /> if the path is fully qualified; otherwise, <see langword="false" />.</returns>
    public partial bool IsPathFullyQualified();

    /// <summary>
    /// Returns a value that indicates whether the path is partially qualified.
    /// A partially qualified path is one that is not fully qualified; it may be relative to the current directory,
    /// or it may not include a drive or server root (e.g., "file.txt", "folder\file.txt", or "C:file.txt").
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if the path is partially qualified; otherwise, <see langword="false" />.
    /// </returns>
    public partial bool IsPartiallyQualified();

    /// <summary>
    /// Returns a value that indicates whether the path contains a root.
    /// </summary>
    /// <returns><see langword="true" /> if the path contains a root; otherwise, <see langword="false" />.</returns>
    public partial bool IsPathRooted();

    /// <summary>
    /// Determines whether the current path represents a root directory.
    /// </summary>
    /// <returns><see langword="true" /> if the path is a root directory; otherwise, <see langword="false" />.</returns>
    public partial bool IsRoot();

    /// <summary>
    /// Normalize separators in the given path. Compresses forward slash runs.
    /// </summary>
    /// <returns><see langword="true" /> if the path was normalized; otherwise, <see langword="false" />.</returns>
    public partial bool NormalizeDirectorySeparators();

    /// <summary>
    /// Compares the current path with the specified path for equality, 
    /// normalizing directory separators and ignoring trailing separators.
    /// </summary>
    /// <param name="path">The path to compare with.</param>
    /// <returns><see langword="true" /> if the paths are equal; otherwise, <see langword="false" />.</returns>
    /// <remarks>
    /// This method performs a character-by-character comparison of the paths, normalizing the directory separators
    /// to match the current system's directory separator character. It also ignores trailing directory separators
    /// to ensure that paths like "C:\Path\" and "C:\Path" are considered equal. However, it does not expand relative paths,
    /// so paths containing ".." or "." will be compared as-is without resolving them to their absolute counterparts.
    /// </remarks>
    public partial bool PathEquals(scoped ReadOnlySpan<char> path);

    /// <summary>
    /// Returns <see langword="true"/> if the path starts in a directory separator.
    /// </summary>
    /// <returns><see langword="true" /> if the path starts in a directory separator; otherwise, <see langword="false" />.</returns>
    public partial bool StartsWithDirectorySeparator();

    /// <summary>
    /// Returns the path.
    /// </summary>
    /// <returns>A string that represents the path.</returns>
    public override partial string ToString();

    /// <summary>
    /// Trims one trailing directory separator beyond the root of the path.
    /// </summary>
    /// <remarks>
    /// This method modifies the current instance of the path by removing exactly one trailing 
    /// directory separator character ('/' or '\\') if present. It does not alter the root portion 
    /// of the path, such as "C:\\" or "/" for Windows and Unix-like systems respectively.
    /// </remarks>
    /// <returns><see langword="true" /> if a trailing directory separator was removed; otherwise, <see langword="false" />.</returns>
    public partial bool TrimEndingDirectorySeparator();

    /// <summary>
    /// Tries to copy the contents of the builder to a destination span.
    /// </summary>
    /// <param name="destination">The destination span to copy to.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters written to the destination span.</param>
    /// <returns><see langword="true"/> if the copy operation was successful; otherwise, <see langword="false"/>.</returns>
    public partial bool TryCopyTo(scoped Span<char> destination, out int charsWritten);
}
