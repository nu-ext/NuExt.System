using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO;

partial class PathBuilder
{
    private char[] _chars;
    private int _pos;
#pragma warning disable IDE0044 // Add readonly modifier
    private char? _directorySeparatorChar;
#pragma warning restore IDE0044 // Add readonly modifier
    private bool? _isUnixLikePlatform;


    private static readonly char[] s_emptyArray = [];

    public partial PathBuilder()
    {
        _chars = s_emptyArray;
        _pos = 0;
    }

    public partial PathBuilder(int initialCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity);

        _chars = initialCapacity == 0 ? s_emptyArray : new char[initialCapacity];
        _pos = 0;
    }

    public partial PathBuilder(IEnumerable<char> initialPath)
    {
        ArgumentNullException.ThrowIfNull(initialPath);

        if (initialPath is ICollection<char> chars)
        {
            int count = chars.Count;
            if (count == 0)
            {
                _chars = s_emptyArray;
                _pos = 0;
            }
            else
            {
                _chars = new char[count];
                chars.CopyTo(_chars, 0);
                _pos = count;
            }
        }
        else
        {
            _chars = s_emptyArray;
            _pos = 0;
            foreach (char c in initialPath)
            {
                Add(c);
            }
        }
    }

    #region Properties

    public partial int Capacity => _chars.Length;

    public partial char DirectorySeparatorChar
    {
        get => _directorySeparatorChar ?? Path.DirectorySeparatorChar;
        set 
        {
            if (value != '/' && value != '\\')
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _directorySeparatorChar = value;
            _isUnixLikePlatform = PathUtilities.IsUnixLikePlatform(value);
        }
    }

    private bool IsUnixLikePlatform => _isUnixLikePlatform ??= PathUtilities.IsUnixLikePlatform(DirectorySeparatorChar);

    public partial int Length
    {
        get => _pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= _chars.Length);
            if ((uint)value > (uint)_chars.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _pos = value;
        }
    }

    public partial ref char this[int index]
    {
        get
        {
            Debug.Assert(index < _pos);
            if ((uint)index >= (uint)_pos)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return ref _chars[index];
        }
    }

    #endregion

    #region Utility Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public partial void Add(char c)
    {
        int pos = _pos;
        var chars = _chars;
        if ((uint)pos < (uint)chars.Length)
        {
            chars[pos] = c;
            _pos = pos + 1;
        }
        else
        {
            AddWithResize(c);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(char c)
    {
        int pos = _pos;
        Grow(1);
        _chars[pos] = c;
        _pos = pos + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //[OverloadResolutionPriority(-1)]
    public partial void Add(scoped ReadOnlySpan<char> s)
    {
        if (s.IsEmpty)
        {
            return;
        }

        int pos = _pos;
        if (pos > _chars.Length - s.Length)
        {
            Grow(s.Length);
        }
#if NETFRAMEWORK || NETSTANDARD2_0
        s.CopyTo(_chars.AsSpan().Slice(_pos));
#else
        s.CopyTo(_chars.AsSpan()[_pos..]);
#endif
        _pos += s.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public partial void Add(string? s)
    {
        if (s == null)
        {
            return;
        }

        int pos = _pos;
        if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // common case
        {
            _chars[pos] = s[0];
            _pos = pos + 1;
        }
        else
        {
            AddSlow(s);
        }
    }

    private void AddSlow(string s)
    {
        int pos = _pos;
        if (pos > _chars.Length - s.Length)
        {
            Grow(s.Length);
        }

        s.CopyTo(0, _chars, _pos, s.Length);
        _pos += s.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public partial void Clear()
    {
        _pos = 0;
#if NET
        Array.Clear(_chars);
#else
        Array.Clear(_chars, 0, _chars.Length);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public partial void Dispose()
    {
        Clear();
    }

    public partial void EnsureCapacity(int capacity)
    {
        // Negative capacity is always invalid, even when the current buffer is empty.
        Debug.Assert(capacity >= 0);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        if (capacity > _chars.Length)
        {
            Grow(capacity - _pos);
        }
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// Does not ensure there is a null char after <see cref="Length"/>
    /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
    /// the explicit method call, and write eg "fixed (char* c = builder)"
    /// </summary>
    public ref readonly char GetPinnableReference()
    {
        return ref MemoryMarshal.GetReference(_chars.AsSpan());
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
    public ref readonly char GetPinnableReference(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(_pos + 1);
            _chars[_pos] = '\0';
        }
        return ref MemoryMarshal.GetReference(_chars.AsSpan());
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="_pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        const uint arrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
        // to double the size if possible, bounding the doubling to not go beyond the max array length.
        int newCapacity = (int)Math.Max(
            (uint)(_pos + additionalCapacityBeyondPos),
            Math.Min((uint)_chars.Length * 2, arrayMaxLength));

        var destinationArray = new char[newCapacity];
        _chars.CopyTo(destinationArray, 0);
        _chars = destinationArray;
    }

    public override partial string ToString()
    {
        return _pos == 0 ? string.Empty : new string(_chars, 0, _pos);
    }

    public partial ReadOnlySpan<char> AsSpan(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(_pos + 1);
            _chars[_pos] = '\0';
        }
#if NETFRAMEWORK || NETSTANDARD2_0
        return _chars.AsSpan().Slice(0, _pos);
#else
        return _chars.AsSpan()[.._pos];
#endif
    }

    public partial ReadOnlySpan<char> AsSpan()
    {
#if NETFRAMEWORK || NETSTANDARD2_0
        return _chars.AsSpan().Slice(0, _pos);
#else
        return _chars.AsSpan()[.._pos];
#endif
    }

    public partial ReadOnlySpan<char> AsSpan(int start)
    {
#if NETFRAMEWORK || NETSTANDARD2_0
        return _chars.AsSpan().Slice(start, _pos - start);
#else
        return _chars.AsSpan()[start.._pos];
#endif
    }

    public partial ReadOnlySpan<char> AsSpan(int start, int length)
    {
        if ((uint)start > (uint)_pos || (uint)length > (uint)(_pos - start))
        {
            throw new ArgumentOutOfRangeException();
        }

        return _chars.AsSpan(start, length);
    }

    public partial bool TryCopyTo(scoped Span<char> destination, out int charsWritten)
    {
        if (!AsSpan().TryCopyTo(destination))
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = _pos;
        return true;
    }

    #endregion

    #region Methods

    public static PathBuilder operator /(PathBuilder builder, scoped ReadOnlySpan<char> path)
    {
        builder.Append(path);
        return builder;
    }

    public static PathBuilder operator /(PathBuilder builder, string? path)
    {
        builder.Append(path);
        return builder;
    }

    public static PathBuilder operator +(PathBuilder builder, scoped ReadOnlySpan<char> s)
    {
        builder.Add(s);
        return builder;
    }

    public static PathBuilder operator +(PathBuilder builder, string? s)
    {
        builder.Add(s);
        return builder;
    }

    //[OverloadResolutionPriority(-1)]
    public partial void Append(scoped ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
        {
            return;
        }

        int pos = _pos;
        if (pos > 0)
        {
            var c = _chars[pos - 1];
            if (!PathUtilities.IsDirectorySeparator(c, IsUnixLikePlatform) && !PathUtilities.IsDirectorySeparator(path[0], IsUnixLikePlatform))
            {
                Add(DirectorySeparatorChar);
            }
        }
        Add(path);
    }

    public partial void Append(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (_pos > 0)
        {
            var c = _chars[_pos - 1];
            if (!PathUtilities.IsDirectorySeparator(c, IsUnixLikePlatform) && !PathUtilities.IsDirectorySeparator(path![0], IsUnixLikePlatform))
            {
                Add(DirectorySeparatorChar);
            }
        }
        Add(path);
    }

    public partial bool ChangeExtension(string? extension)
    {
        int subLength = _pos;
        if (subLength == 0)
        {
            return false;
        }

        for (int i = _pos - 1; i >= 0; i--)
        {
            char c = _chars[i];
            if (c == '.')
            {
                subLength = i;
                break;
            }
            if (PathUtilities.IsDirectorySeparator(c, IsUnixLikePlatform))
            {
                break;
            }
        }

        int pos = _pos;
        _pos = subLength;

        if (extension == null)
        {
            return subLength < pos;
        }

        if (!extension.StartsWith('.'))
        {
            Add('.');
        }
        Add(extension);
        return true;
    }

    //[OverloadResolutionPriority(-1)]
    public partial bool ChangeFileName(scoped ReadOnlySpan<char> newFileName)
    {
        var fileNameLength = GetFileName().Length;

        _pos -= fileNameLength;

        if (newFileName.IsEmpty)
        {
            return fileNameLength > 0;
        }

        Append(newFileName);
        return fileNameLength > 0 || newFileName.Length > 0;
    }

    public partial bool ChangeFileName(string? newFileName)
    {
        var fileNameLength = GetFileName().Length;

        _pos -= fileNameLength;

        if (newFileName == null)
        {
            return fileNameLength > 0;
        }

        Append(newFileName);
        return fileNameLength > 0 || newFileName.Length > 0;
    }

    public partial bool EndsInDirectorySeparator()
    {
        return _pos > 0 && PathUtilities.IsDirectorySeparator(_chars[_pos - 1], IsUnixLikePlatform);
    }

    public partial bool EnsureTrailingSeparator()
    {
        if (EndsInDirectorySeparator())
        {
            return false;
        }
        Add(DirectorySeparatorChar);
        return true;
    }

    public partial ReadOnlySpan<char> GetDirectoryName()
    {
        return PathUtilities.GetDirectoryName(AsSpan(), IsUnixLikePlatform);
    }

    public partial ReadOnlySpan<char> GetExtension()
    {
        return PathUtilities.GetExtension(AsSpan(), IsUnixLikePlatform);
    }

    public partial ReadOnlySpan<char> GetFileName()
    {
        return PathUtilities.GetFileName(AsSpan(), IsUnixLikePlatform);
    }

    public partial ReadOnlySpan<char> GetFileNameWithoutExtension()
    {
        return PathUtilities.GetFileNameWithoutExtension(AsSpan(), IsUnixLikePlatform);
    }

    public partial ReadOnlySpan<char> GetFullPath()
    {
        return PathUtilities.GetFullPath(AsSpan(), IsUnixLikePlatform);
    }

    public partial ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> basePath)
    {
        return PathUtilities.GetFullPath(AsSpan(), basePath, IsUnixLikePlatform);
    }

    public partial ReadOnlySpan<char> GetPathRoot()
    {
        return PathUtilities.GetPathRoot(AsSpan(), IsUnixLikePlatform);
    }

    public partial int GetPathSegments(ICollection<string>? segments)
    {
        return PathUtilities.GetPathSegments(AsSpan(), segments, IsUnixLikePlatform);
    }

    public partial ReadOnlySpan<char> GetRelativePath(ReadOnlySpan<char> path, ReadOnlySpan<char> basePath)
    {
        return PathUtilities.GetRelativePath(AsSpan(), path, IsUnixLikePlatform, basePath);
    }

    public partial bool HasExtension()
    {
        return PathUtilities.HasExtension(AsSpan(), IsUnixLikePlatform);
    }

    public partial bool IsEffectivelyEmpty()
    {
        return PathUtilities.IsEffectivelyEmpty(AsSpan(), IsUnixLikePlatform);
    }

    public partial bool IsPathFullyQualified()
    {
        return PathUtilities.IsPathFullyQualified(AsSpan(), IsUnixLikePlatform);
    }

    public partial bool IsPartiallyQualified()
    {
        return PathUtilities.IsPartiallyQualified(AsSpan(), IsUnixLikePlatform);
    }

    public partial bool IsPathRooted()
    {
        return PathUtilities.IsPathRooted(AsSpan(), IsUnixLikePlatform);
    }

    public partial bool IsRoot()
    {
        return PathUtilities.IsRoot(AsSpan(), IsUnixLikePlatform);
    }

    public partial bool NormalizeDirectorySeparators()
    {
        if (_pos == 0)
        {
            return false;
        }
        ValueStringBuilder builder = default;
        var result = PathUtilities.NormalizeDirectorySeparators(AsSpan(), ref builder, IsUnixLikePlatform);
        if (result)
        {
            Debug.Assert(builder.Length > 0);
            _pos = 0;
            Add(builder.AsSpan());
        }
        builder.Dispose();
        return result;
    }

    public partial bool PathEquals(scoped ReadOnlySpan<char> path)
    {
        return PathUtilities.PathEquals(AsSpan(), path, IsUnixLikePlatform);
    }

    public partial bool StartsWithDirectorySeparator()
    {
        return _pos > 0 && PathUtilities.IsDirectorySeparator(_chars[0], IsUnixLikePlatform);
    }

    public partial bool TrimEndingDirectorySeparator()
    {
        if (EndsInDirectorySeparator() && !IsRoot())
        {
            _pos -= 1;
            return true;
        }
        return false;
    }

    #endregion
}
