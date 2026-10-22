using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System.IO;

partial class IOUtils
{
    private static readonly HashSet<char> s_invalidFileNameCharsSet = [..Path.GetInvalidFileNameChars()];
    private static readonly char[] s_trimFileNameChars = [' ', '\t', '_', '-'];

    /// <summary>
    /// Sanitizes a file name by replacing OS-specific invalid characters and optionally truncates its length.
    /// </summary>
    /// <remarks>
    /// <para>The set of invalid characters is determined by <see cref="Path.GetInvalidFileNameChars"/> and varies by operating system.</para>
    /// <para>This method returns an empty string if the input is null or empty.</para>
    /// </remarks>
    /// <param name="fileName">The original file name. Can be null.</param>
    /// <param name="maxLength">The maximum allowed length of the resulting file name including its extension. If 0, no truncation is performed.</param>
    /// <param name="replacementChar">The character to replace invalid characters with (default is '_').</param>
    /// <returns>A sanitized file name. Returns <see cref="string.Empty"/> if the input is null or empty.</returns>
    public static string SanitizeFileName(string? fileName, int maxLength = 0, char replacementChar = '_')
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return string.Empty;
        }

        if (s_invalidFileNameCharsSet.Contains(replacementChar))
        {
            replacementChar = '_';
        }

        var sb = new StringBuilder(fileName!.Length);
        foreach (var c in fileName)
        {
            sb.Append(s_invalidFileNameCharsSet.Contains(c) ? replacementChar : c);
        }
        string sanitized = sb.ToString();

        if (maxLength > 0 && sanitized.Length > maxLength)
        {
            sanitized = TruncateFileNamePreservingExtension(sanitized, maxLength);
        }

        return sanitized;
    }

    /// <summary>
    /// Gets file size in bytes of a file at the specified path.
    /// </summary>
    /// <param name="fullPath">The full path of the file.</param>
    /// <returns>The file size in bytes.</returns>
    /// <exception cref="IOException">Thrown when an IO error occurs while accessing the file.</exception>
    /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
    public static long GetFileSize(string fullPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(fullPath);

        var info = new FileInfo(fullPath);
        return info.Length;
    }

    /// <summary>
    /// Attempts to get the size of the specified file.
    /// </summary>
    /// <param name="fullPath">The path of the file.</param>
    /// <param name="size">
    /// When this method returns <see langword="true"/>, contains the file size in bytes;
    /// otherwise, <c>-1</c>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the file exists and its size was retrieved successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="fullPath"/> is <see langword="null"/> or empty.
    /// </exception>
    public static bool TryGetFileSize(string fullPath, out long size)
    {
        ArgumentException.ThrowIfNullOrEmpty(fullPath);

        try
        {
            var info = new FileInfo(fullPath);
            if (info.Exists)
            {
                size = info.Length;
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.Assert(false, ex.Message);
        }

        size = -1;
        return false;
    }

    /// <summary>
    /// Finds a free file name by appending a numeric suffix to the base file name if necessary.
    /// </summary>
    /// <param name="path">The original file path.</param>
    /// <param name="format">The format string used to generate the new file name. Must contain three placeholders: {0} for the base name, {1} for the numeric suffix, and {2} for the extension. Default is "{0} ({1}){2}".</param>
    /// <returns>A free file path that does not currently exist.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when unable to find a free file name after a maximum number of attempts.</exception>
    public static string GetFreeFileName(string path, string format = "{0} ({1}){2}")
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return path;
        }

        var template = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        var dir = Path.GetDirectoryName(path) ?? throw new ArgumentException("Invalid path", nameof(path));

        var sb = new StringBuilder(template.Length + ext.Length + 16);

        const int maxAttempts = 1000;
        int i = 1;
        string newPath;
        do
        {
            sb.Length = 0;
            sb.AppendFormat(CultureInfo.InvariantCulture, format, template, i, ext);
            string fileName = sb.ToString();
            newPath = Path.Combine(dir, fileName);
        } while ((File.Exists(newPath) || Directory.Exists(newPath)) && ++i <= maxAttempts);
        if (i > maxAttempts)
        {
            Throw.InvalidOperationException($"Unable to find a free file name after {maxAttempts} attempts.");
        }
        return newPath;
    }

    /// <summary>
    /// Trims a file name intelligently to fit within the specified limit size, preserving word boundaries where possible.
    /// </summary>
    /// <param name="fileName">The original file name.</param>
    /// <param name="limitSize">The maximum allowed length of the file name including its extension.</param>
    /// <returns>The trimmed file name.</returns>
    public static string TruncateFileNamePreservingExtension(string fileName, int limitSize)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        if (limitSize <= 0 || fileName.Length <= limitSize)
        {
            return fileName;
        }

        var extension = Path.GetExtension(fileName);

        if (extension.Length >= limitSize)
        {
#if NET || NETSTANDARD2_1_OR_GREATER
            return extension.Length > limitSize ? extension[..limitSize] : extension;
#else
            return extension.Length > limitSize ? extension.Substring(0, limitSize) : extension;
#endif
        }

#if NET || NETSTANDARD2_1_OR_GREATER
        var nameWithoutExtension = fileName[..^extension.Length];
#else
        var nameWithoutExtension = fileName.Substring(0, fileName.Length - extension.Length);
#endif

        int count = limitSize - extension.Length;
        var index = nameWithoutExtension.LastIndexOfAny(s_trimFileNameChars, count);
#if NET || NETSTANDARD2_1_OR_GREATER
        nameWithoutExtension = index > 0 ? nameWithoutExtension[..index] : nameWithoutExtension[..count];
#else
        nameWithoutExtension = index > 0 ? nameWithoutExtension.Substring(0, index) : nameWithoutExtension.Substring(0, count);
#endif
        return nameWithoutExtension + extension;
    }

    /// <summary>
    /// Safely deletes a file if it exists.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    /// <param name="exception">An exception.</param>
    /// <returns><see langword="true"/> if the file was successfully deleted or did not exist, false if an error occurred.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
    public static bool TryDeleteFile(string filePath, out Exception? exception)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            exception = null;
            return true;
        }
        catch (Exception ex)
        {
            Debug.Assert(false, ex.Message);
            exception = ex;
            return false;
        }
    }
}
