// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

[SuppressMessage("Style", "IDE0057:Slice can be simplified", Justification = "<Pending>")]
public ref partial struct ValueListBuilder<T>
{
    private Span<T> _span;
    private T[]? _arrayFromPool;
    private int _pos;

    public ValueListBuilder(Span<T?> scratchBuffer)
    {
        _span = scratchBuffer!;
    }

    public ValueListBuilder(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity > 0) Grow(capacity);
    }

    public int Length
    {
        readonly get => _pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= _span.Length);
            if ((uint)value > (uint)_span.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _pos = value;
        }
    }

    public ref readonly T this[int index]
    {
        get
        {
            Debug.Assert(index < _pos);
            if ((uint)index >= (uint)_pos)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return ref _span[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(T item)
    {
        int pos = _pos;

        // Workaround for https://github.com/dotnet/runtime/issues/72004
        Span<T> span = _span;
        if ((uint)pos < (uint)span.Length)
        {
            span[pos] = item;
            _pos = pos + 1;
        }
        else
        {
            AddWithResize(item);
        }
    }

    //[OverloadResolutionPriority(-1)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<T> source)
    {
        int pos = _pos;
        Span<T> span = _span;
        if (source.Length == 1 && (uint)pos < (uint)span.Length)
        {
            span[pos] = source[0];
            _pos = pos + 1;
        }
        else
        {
            AppendMultiChar(source);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendMultiChar(scoped ReadOnlySpan<T> source)
    {
        if ((uint)(_pos + source.Length) > (uint)_span.Length)
        {
            Grow(source.Length);
        }

        source.CopyTo(_span.Slice(_pos));
        _pos += source.Length;
    }

    public void Insert(int index, scoped ReadOnlySpan<T> source)
    {
        Debug.Assert((uint)index <= (uint)_pos, "Index should be in range [0, _pos]");

        if ((uint)index > (uint)_pos)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (source.IsEmpty)
        {
            return;
        }

        if (source.Overlaps(_span))
        {
            source = source.ToArray();
        }

        if ((uint)(_pos + source.Length) > (uint)_span.Length)
        {
            Grow(source.Length);
        }

        _span.Slice(index, _pos - index).CopyTo(_span.Slice(index + source.Length));
        source.CopyTo(_span.Slice(index));
        _pos += source.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AppendSpan(int length)
    {
        Debug.Assert(length >= 0);

        int pos = _pos;
        Span<T> span = _span;
        if ((uint)(pos + length) <= (uint)span.Length)
        {
            _pos = pos + length;
            return span.Slice(pos, length);
        }
        else
        {
            return AppendSpanWithGrow(length);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Span<T> AppendSpanWithGrow(int length)
    {
        int pos = _pos;
        Grow(length);
        _pos += length;
        return _span.Slice(pos, length);
    }

    // Hide uncommon path
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        Debug.Assert(_pos == _span.Length);
        int pos = _pos;
        Grow(1);
        _span[pos] = item;
        _pos = pos + 1;
    }

    public readonly ReadOnlySpan<T> AsSpan()
    {
        return _span.Slice(0, _pos);
    }

    public readonly bool TryCopyTo(Span<T> destination, out int itemsWritten)
    {
        if (_span.Slice(0, _pos).TryCopyTo(destination))
        {
            itemsWritten = _pos;
            return true;
        }

        itemsWritten = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        T[]? toReturn = _arrayFromPool;
        int length = _span.Length;
        this = default;

        if (toReturn != null)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(toReturn, 0, length);
            }

            ArrayPool<T>.Shared.Return(toReturn);
        }
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="_pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    /// <remarks>
    /// Note that consuming implementations depend on the list only growing if it's absolutely
    /// required.  If the list is already large enough to hold the additional items be added,
    /// it must not grow. The list is used in a number of places where the reference is checked
    /// and it's expected to match the initial reference provided to the constructor if that
    /// span was sufficiently large.
    /// </remarks>
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(_pos > _span.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        const int ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Double the size of the span.  If it's currently empty, default to size 4,
        // although it'll be increased in Rent to the pool's minimum bucket size.
        int nextCapacity = Math.Max(_span.Length != 0 ? _span.Length * 2 : 4, _pos + additionalCapacityBeyondPos);

        // If the computed doubled capacity exceeds the possible length of an array, then we
        // want to downgrade to either the maximum array length if that's large enough to hold
        // an additional item, or the current length + 1 if it's larger than the max length, in
        // which case it'll result in an OOM when calling Rent below.  In the exceedingly rare
        // case where _span.Length is already int.MaxValue (in which case it couldn't be a managed
        // array), just use that same value again and let it OOM in Rent as well.
        if ((uint)nextCapacity > ArrayMaxLength)
        {
            nextCapacity = Math.Max(Math.Max(_span.Length + 1, ArrayMaxLength), _span.Length);
        }

        T[] array = ArrayPool<T>.Shared.Rent(nextCapacity);
        _span.CopyTo(array);

        T[]? toReturn = _arrayFromPool;
        int oldLength = _span.Length;
        _span = _arrayFromPool = array;
        if (toReturn != null)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(toReturn, 0, oldLength);
            }

            ArrayPool<T>.Shared.Return(toReturn);
        }
    }

    public T[] ToArray()
    {
        T[] array = _pos == 0 ? [] : _span.Slice(0, _pos).ToArray();
        Dispose();
        return array;
    }
}
