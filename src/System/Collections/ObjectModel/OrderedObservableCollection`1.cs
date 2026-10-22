using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.ObjectModel;

/// <summary>
/// Represents a collection of objects that maintains an observable order,
/// where each object implements the <see cref="IOrdered"/> interface.
/// </summary>
/// <typeparam name="T">The type of elements in the collection, which must implement <see cref="IOrdered"/>.</typeparam>
/// <remarks>
/// The collection maintains item order after each mutation. During a synchronous
/// <see cref="System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged"/> notification, affected
/// item order values may still reflect their state before the mutation.
/// </remarks>
public class OrderedObservableCollection<T> : ObservableCollection<T> where T : IOrdered
{
    /// <summary>
    /// Initializes a new instance of OrderedObservableCollection that is empty and has default initial capacity.
    /// </summary>
    public OrderedObservableCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the OrderedObservableCollection class that contains
    /// elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    /// <exception cref="ArgumentNullException">collection is a null reference</exception>
    public OrderedObservableCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OrderedObservableCollection class
    /// that contains elements copied from the specified list
    /// </summary>
    /// <param name="list">The list whose elements are copied to the new list.</param>
    /// <exception cref="ArgumentNullException">list is a null reference</exception>
    public OrderedObservableCollection(List<T> list) : base(list)
    {
    }

    /// <inheritdoc/>
    protected override void InsertItem(int index, T item)
    {
        CheckReentrancy();

        base.InsertItem(index, item);

        // Neighbors after insertion
        int leftIndex = index - 1;
        int rightIndex = index + 1;

        bool hasLeft = leftIndex >= 0;
        bool hasRight = rightIndex < Count;

        InsertOrMoveItem(item, index, leftIndex, rightIndex, hasLeft, hasRight);
    }
    
    /// <inheritdoc/>
    protected override void SetItem(int index, T item)
    {
        CheckReentrancy();
        item.Order = this[index].Order;
        base.SetItem(index, item);
    }

    /// <inheritdoc/>
    protected override void MoveItem(int oldIndex, int newIndex)
    {
        CheckReentrancy();

        if (oldIndex == newIndex)
        {
            return;
        }

        T item = this[oldIndex];
        T targetItem = this[newIndex];

        base.MoveItem(oldIndex, newIndex);

        // Neighbors after moving
        int leftIndex = newIndex - 1;
        int rightIndex = newIndex + 1;

        bool hasLeft = leftIndex >= 0;
        bool hasRight = rightIndex < Count;

        // Adjacent swap — ONLY when the moved item is internal (has both neighbors)
        if (Math.Abs(oldIndex - newIndex) == 1 && hasLeft && hasRight)
        {
            SwapOrders(item, targetItem);
            return;
        }

        InsertOrMoveItem(item, newIndex, leftIndex, rightIndex, hasLeft, hasRight);
    }

    private void InsertOrMoveItem(T item, int index, int leftIndex, int rightIndex, bool hasLeft, bool hasRight)
    {
        int leftOrder = hasLeft ? this[leftIndex].Order : 0;
        int rightOrder = hasRight ? this[rightIndex].Order : int.MaxValue;

        (int chunk, int step) = GetScale();
        Debug.Assert(step >= chunk, "step must be >= chunk.");

        // Edge: to the end (no right neighbor)
        if (!hasRight)
        {
            item.Order = leftOrder - (leftOrder % chunk) + step;
            return;
        }

        // Edge: to the start (no left neighbor)
        if (!hasLeft)
        {
            int preferred = rightOrder - (rightOrder % chunk) - chunk;
            preferred = EnsureNonNegative(preferred, 0);

            if (preferred >= rightOrder)
            {
                // Cannot place strictly below 'right' -> normalize
                NormalizeOrders();
            }
            else
            {
                item.Order = preferred;
            }
            return;
        }

        // Inside (between two neighbors)
        int gap = rightOrder - leftOrder;
        if (gap <= 1)
        {
            // No room -> normalize forward from the item
            NormalizeOrders(index);
            return;
        }

        // Try a pretty value near the midpoint (floor to chunk)
        int mid = leftOrder + (gap / 2);

        int pretty = mid - (mid % chunk);
        if (pretty <= leftOrder)
        {
            pretty += chunk;
            if (pretty >= rightOrder) pretty = mid;
        }

        item.Order = pretty;
    }

    /// <summary>
    /// Normalizes the Order properties of items in the collection with minimal updates.
    /// Ensures strictly increasing sequence with at least one chunk gap, adjusting only where necessary.
    /// </summary>
    public void NormalizeOrders()
    {
        NormalizeOrders(0);
    }

    /// <summary>
    /// Normalizes the Order properties of items in the collection with minimal updates starting from the specified index.
    /// </summary>
    /// <param name="startIndex">
    /// Index from which the normalization should start (inclusive).
    /// Values before this index are treated as already fixed.
    /// </param>

    public void NormalizeOrders(int startIndex)
    {
        if (Count <= 1 || startIndex >= Count)
        {
            return;
        }

        if (startIndex < 0) startIndex = 0;

        (int chunk, int step) = GetScale();
        Debug.Assert(step >= chunk, "step must be >= chunk.");

        T lastItem = this[Count - 1];
        T firstItem = this[startIndex];

        bool hasGrownToStep = lastItem.Order >= step;

        int previousOrder;
        if (startIndex == 0)
        {
            if (!hasGrownToStep && firstItem.Order < chunk)
            {
                firstItem.Order = chunk;
            }
            else
            {
                firstItem.Order = EnsureNonNegative(firstItem.Order, chunk);
            }
            previousOrder = firstItem.Order;
            startIndex = 1;
        }
        else
        {
            previousOrder = this[startIndex - 1].Order;
        }

        for (int i = startIndex; i < Count; i++)
        {
            T item = this[i];

            if (item.Order > previousOrder)
            {
                //nothing to change
                previousOrder = item.Order;
                continue;
            }

            if (hasGrownToStep)
            {
                int order = previousOrder - previousOrder % chunk + chunk;
                item.Order = previousOrder = order;
                continue;
            }

            if (previousOrder < step)
            {
                int order = (i + 1) * step;
                item.Order = previousOrder = order;
                continue;
            }

            item.Order = previousOrder = (previousOrder - previousOrder % chunk + step);
        }

#if DEBUG
        previousOrder = this[startIndex - 1].Order;
        var seen = new HashSet<int>();
        for (int i = startIndex; i < Count; i++)
        {
            var item = this[i];
            Debug.Assert(item.Order > previousOrder);
            Debug.Assert(item.Order >= 0);
            Debug.Assert(seen.Add(item.Order));
            previousOrder = item.Order;
        }
#endif
    }

    /// <summary>
    /// Validates collection order invariants starting from the specified index (inclusive).
    /// Invariants:
    ///  - Order >= 0
    ///  - Strictly increasing
    /// Optional:
    ///  - If requireChunkAlignment == true and the collection has grown to 'step',
    ///    all orders must be aligned to 'chunk'.
    /// Throws InvalidOperationException on the first violation.
    /// </summary>
    public void ValidateOrders(int startIndex = 0, bool requireChunkAlignment = false)
    {
        if (Count == 0 || startIndex >= Count)
        {
            return;
        }
        if (startIndex < 0) startIndex = 0;

        (int chunk, int step) = GetScale();
        bool hasGrownToStep = this[Count - 1].Order >= step;

        int prev = (startIndex > 0) ? this[startIndex - 1].Order : int.MinValue;

        for (int i = startIndex; i < Count; i++)
        {
            int item = this[i].Order;

            if (item < 0)
                throw new InvalidOperationException($"Order must be >= 0. Index={i}, Order={item}");

            if (item <= prev)
                throw new InvalidOperationException($"Order must be strictly increasing. Index={i}, Prev={prev}, Order={item}");

            if (requireChunkAlignment && hasGrownToStep && (item % chunk != 0))
                throw new InvalidOperationException($"Order must be aligned to chunk={chunk}. Index={i}, Order={item}");

            prev = item;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual (int chunk, int step) GetScale()
    {
        return Count switch
        {
            < 1000 => (100, 10_000),    // 100-999 items
            < 10_000 => (50, 1_000),    // 1k-10k items
            < 100_000 => (10, 100),     // 10k-100k items
            < 1_000_000 => (5, 10),     // 100k-1m items
            _ => (2, 5)                 // >1m items
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EnsureNonNegative(int value, int min) => value >= 0 ? value : Math.Max(min, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SwapOrders(T item1, T item2)
    {
        (item1.Order, item2.Order) = (item2.Order, item1.Order);
    }

}
