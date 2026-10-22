namespace System;

/// <summary>
/// Provides utility methods for formatting.
/// </summary>
public static class FormatUtils
{
    private const long _1KB = 1024;
    private const long _1MB = 1024 * _1KB;
    private const long _1GB = 1024 * _1MB;
    private const long _1TB = 1024 * _1GB;

    /// <summary>
    /// Formats a size in bytes into a human-readable string using the appropriate unit (B, KB, MB, GB, or TB).
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="size">The size in bytes to be formatted.</param>
    /// <returns>A human-readable string representation of the size with the appropriate unit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="size"/> is negative.
    /// </exception>
    public static string FormatSize(IFormatProvider? provider, long size)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(size);

        FormattableString message = size switch
        {
            < _1KB => $"{size} B",
            < _1MB => $"{(double)size / _1KB:N1} KB",
            < _1GB => $"{(double)size / _1MB:N1} MB",
            < _1TB => $"{(double)size / _1GB:N1} GB",
            _ => $"{(double)size / _1TB:N1} TB"
        };
        return message.ToString(provider);
    }
}
