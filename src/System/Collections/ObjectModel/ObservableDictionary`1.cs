using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.ObjectModel;

/// <summary>Represents a dynamic keyed collection that provides notifications when items get added, removed, or updated, or when the whole collection is refreshed.</summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <remarks>
/// This type is not thread-safe. All members are unsynchronized and must be accessed from a single thread.
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class ObservableDictionary<TKey, TValue>(int capacity, IEqualityComparer<TKey>? comparer) : INotifyPropertyChanged, INotifyCollectionChanged,
    IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IList
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _innerDictionary = new(capacity, comparer);

    private SimpleMonitor? _monitor; // Lazily allocated only when a subclass calls BlockReentrancy()
    private int _blockReentrancyCount;

    // Maintains a stable key order and an index map for O(1) index lookup while preserving order.
    private readonly List<TKey> _orderedKeys = new(capacity);
    private readonly Dictionary<TKey, int> _indexMap = new(capacity, comparer);

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
    /// </summary>
    public ObservableDictionary() : this(0, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class
    /// with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the dictionary can contain.</param>
    public ObservableDictionary(int capacity) : this(capacity, null) { }

    public ObservableDictionary(IEqualityComparer<TKey>? comparer) : this(0, comparer) { }

    //Collection ctors

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer) :
        this(dictionary?.Count ?? 0, comparer)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        AddRange(dictionary);
    }

    public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, null) { }

    public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer) :
        this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
    {
        ArgumentNullException.ThrowIfNull(collection);

        AddRange(collection);
    }

    #region Properties

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Comparer"/>
    public IEqualityComparer<TKey> Comparer => _innerDictionary.Comparer;

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Count"/>
    public int Count => _innerDictionary.Count;

#if NET9_0_OR_GREATER
    /// <inheritdoc cref="Dictionary{TKey, TValue}.Capacity"/>
    public int Capacity => _innerDictionary.Capacity;
#endif

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Keys"/>
    public Dictionary<TKey, TValue>.KeyCollection Keys => _innerDictionary.Keys;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Values"/>
    public Dictionary<TKey, TValue>.ValueCollection Values => _innerDictionary.Values;

    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    /// <inheritdoc cref="Dictionary{TKey, TValue}.this[TKey]"/>
    public TValue this[TKey key]
    {
        get => _innerDictionary[key];
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            CheckReentrancy();

            bool isExist = _innerDictionary.TryGetValue(key, out var oldValue);
            if (!isExist || !EqualityComparer<TValue>.Default.Equals(oldValue, value))
            {
                _innerDictionary[key] = value;

                if (!isExist)
                {
                    OnCountPropertyChanged();
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    OnIndexerPropertyChanged();
                    OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, oldValue!), new KeyValuePair<TKey, TValue>(key, value));
                }
#if DEBUG
                ValidateState();
#endif
            }
        }
    }

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;


    #endregion

    #region Events

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Methods

    /// <summary>
    /// Adds multiple key-value pairs to the dictionary with a single notification.
    /// </summary>
    /// <param name="items">The key-value pairs to add.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="items" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        CheckReentrancy();

        int capacity = 0;
        if (items is ICollection<KeyValuePair<TKey, TValue>> collection)
        {
            capacity = collection.Count;
            if (capacity == 0) return;
        }

        var tempSet = new HashSet<TKey>(_innerDictionary.Comparer);
        var builder = capacity > 8
            ? new ValueListBuilder<KeyValuePair<TKey, TValue>>(capacity)
            : new ValueListBuilder<KeyValuePair<TKey, TValue>>([default, default, default, default, default, default, default, default]);
        try
        {
            foreach (var pair in items)
            {
                if (!tempSet.Add(pair.Key) || _innerDictionary.ContainsKey(pair.Key))
                {
                    ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException(pair.Key);
                }

                builder.Append(pair);
            }

            if (builder.Length == 0)
            {
                return;
            }

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            _innerDictionary.EnsureCapacity(_innerDictionary.Count + builder.Length);
#endif
            if (builder.Length == 1)
            {
                var first = builder[0];
                _innerDictionary.Add(first.Key, first.Value);

                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Add, first);
            }
            else
            {
                foreach (var pair in builder.AsSpan())
                {
                    _innerDictionary.Add(pair.Key, pair.Value);
                }
#if NET5_0_OR_GREATER
                _orderedKeys.EnsureCapacity(_orderedKeys.Count + builder.Length);
#endif
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                _indexMap.EnsureCapacity(_indexMap.Count + builder.Length);
#endif
                foreach (var pair in builder.AsSpan())
                {
                    _indexMap[pair.Key] = _orderedKeys.Count;
                    _orderedKeys.Add(pair.Key);
                }

                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
            }
#if DEBUG
            ValidateState();
#endif
        }
        finally
        {
            builder.Dispose();
        }
    }

    /// <summary>
    /// Removes multiple keys from the dictionary with a single notification.
    /// Keys that don't exist are silently ignored.
    /// </summary>
    /// <param name="keys">The keys to remove.</param>
    /// <returns>An array of key-value pairs that were actually removed.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="keys" /> is <see langword="null" />.</exception>
    public KeyValuePair<TKey, TValue>[] RemoveRange(IEnumerable<TKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        CheckReentrancy();

        var keyList = new List<TKey>(keys);
        var builder = new ValueListBuilder<KeyValuePair<TKey, TValue>>([default, default, default, default, default, default, default, default]);
        try
        {
            foreach (var key in keyList)
            {
                if (_innerDictionary.Remove(key, out var value))
                {
                    builder.Append(new KeyValuePair<TKey, TValue>(key, value));
                }
            }

            if (builder.Length == 0)
            {
                return [];
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            if (builder.Length == 1)
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, builder[0]);
            }
            else
            {
                _indexMap.Clear();
                bool removed = true;
                foreach (var pair in builder.AsSpan())
                {
                    removed &= _orderedKeys.Remove(pair.Key);
                }

                Debug.Assert(removed);

                for (int i = 0; i < _orderedKeys.Count; i++)
                {
                    _indexMap[_orderedKeys[i]] = i;
                }

                OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
            }
#if DEBUG
            ValidateState();
#endif
            return builder.ToArray();
        }
        finally
        {
            builder.Dispose();
        }
    }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Add"/>
    public void Add(TKey key, TValue value)
    {
        CheckReentrancy();

        _innerDictionary.Add(key, value);

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
#if DEBUG
        ValidateState();
#endif
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) =>
        Add(keyValuePair.Key, keyValuePair.Value);

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
    {
        ICollection<KeyValuePair<TKey, TValue>> dict = _innerDictionary;
        return dict.Contains(keyValuePair);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
    {
        CheckReentrancy();

        ICollection<KeyValuePair<TKey, TValue>> dict = _innerDictionary;
        if (!dict.Remove(keyValuePair)) return false;

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, keyValuePair);
#if DEBUG
        ValidateState();
#endif
        return true;
    }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Clear"/>
    public void Clear()
    {
        CheckReentrancy();

        if (_innerDictionary.Count <= 0) return;

        _innerDictionary.Clear();
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionReset();
#if DEBUG
        ValidateState();
#endif
    }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsKey"/>
    public bool ContainsKey(TKey key) => _innerDictionary.ContainsKey(key);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsValue"/>
    public bool ContainsValue(TValue value) => _innerDictionary.ContainsValue(value);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.GetEnumerator"/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        // Enumerate in the stable order maintained by _orderedKeys.
        foreach (var key in _orderedKeys)
        {
            yield return new KeyValuePair<TKey, TValue>(key, _innerDictionary[key]);
        }
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Remove(TKey)"/>
    public bool Remove(TKey key)
    {
        CheckReentrancy();

        if (!_innerDictionary.Remove(key, out var value)) return false;

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
#if DEBUG
        ValidateState();
#endif
        return true;
    }

#if !(NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
    /// <inheritdoc cref="DictionaryExtensions.Remove"/>
#else
    /// <inheritdoc cref="Dictionary{TKey, TValue}.Remove(TKey, out TValue)"/>
#endif
    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        CheckReentrancy();

        if (!_innerDictionary.Remove(key, out value)) return false;

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
#if DEBUG
        ValidateState();
#endif
        return true;
    }


    /// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
    public bool TryGetValue(TKey key,
#if NET
        [MaybeNullWhen(false)]
#endif
        out TValue value) => _innerDictionary.TryGetValue(key, out value);


#if !(NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
    /// <inheritdoc cref="DictionaryExtensions.TryAdd"/>
#else
    /// <inheritdoc cref="Dictionary{TKey, TValue}.TryAdd(TKey, TValue)"/>
#endif
    public bool TryAdd(TKey key, TValue value)
    {
        CheckReentrancy();

        if (!_innerDictionary.TryAdd(key, value)) return false;

        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
#if DEBUG
        ValidateState();
#endif
        return true;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    bool IList.IsFixedSize => true;

    bool IList.IsReadOnly => true;

    object? IList.this[int index]
    {
        get
        {
            var key = _orderedKeys[index];
            return new KeyValuePair<TKey, TValue>(key, _innerDictionary[key]);
        }
        set => ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
        // Copy in the stable order maintained by _orderedKeys.
        for (int i = 0; i < _orderedKeys.Count; i++)
        {
            var key = _orderedKeys[i];
            array[index++] = new KeyValuePair<TKey, TValue>(key, _innerDictionary[key]);
        }
    }

    void ICollection.CopyTo(Array array, int index)
    {
        // Copy in the stable order maintained by _orderedKeys.
        if (array is KeyValuePair<TKey, TValue>[] kvpArray)
        {
            for (int i = 0; i < _orderedKeys.Count; i++)
            {
                var key = _orderedKeys[i];
                kvpArray[index++] = new KeyValuePair<TKey, TValue>(key, _innerDictionary[key]);
            }
            return;
        }
        if (array is object[] objArray)
        {
            for (int i = 0; i < _orderedKeys.Count; i++)
            {
                var key = _orderedKeys[i];
                objArray[index++] = new KeyValuePair<TKey, TValue>(key, _innerDictionary[key]);
            }
            return;
        }
        ThrowHelper.ThrowArgumentException_Argument_IncompatibleArrayType();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#if (NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
    /// <inheritdoc cref="Dictionary{TKey, TValue}.EnsureCapacity"/>
    public int EnsureCapacity(int capacity) => _innerDictionary.EnsureCapacity(capacity);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.TrimExcess()"/>
    public void TrimExcess() => _innerDictionary.TrimExcess();

    /// <inheritdoc cref="Dictionary{TKey, TValue}.TrimExcess(int)"/>
    public void TrimExcess(int capacity) => _innerDictionary.TrimExcess(capacity);
#endif

    /// <summary>
    /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raise CollectionChanged event to any listeners.
    /// Properties/methods modifying this ObservableCollection will raise
    /// a collection changed event through this virtual method.
    /// </summary>
    /// <remarks>
    /// When overriding this method, either call its base implementation
    /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
    /// </remarks>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        NotifyCollectionChangedEventHandler? handler = CollectionChanged;
        if (handler != null)
        {
            // Not calling BlockReentrancy() here to avoid the SimpleMonitor allocation.
            _blockReentrancyCount++;
            try
            {
                handler(this, e);
            }
            finally
            {
                _blockReentrancyCount--;
            }
        }
    }

    /// <summary>
    /// Disallow reentrant attempts to change this collection. E.g. an event handler
    /// of the CollectionChanged event is not allowed to make changes to this collection.
    /// </summary>
    /// <remarks>
    /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
    /// <code>
    ///         using (BlockReentrancy())
    ///         {
    ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
    ///         }
    /// </code>
    /// </remarks>
    protected IDisposable BlockReentrancy()
    {
        _blockReentrancyCount++;
        return EnsureMonitorInitialized();
    }

    /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
    /// <exception cref="InvalidOperationException"> raised when changing the collection
    /// while another collection change is still being notified to other listeners </exception>
    protected void CheckReentrancy()
    {
        if (_blockReentrancyCount > 0)
        {
            // we can allow changes if there's only one listener - the problem
            // only arises if reentrant changes make the original event args
            // invalid for later listeners.  This keeps existing code working
            // (e.g. Selector.SelectedItems).
            NotifyCollectionChangedEventHandler? handler = CollectionChanged;
            if (handler != null && !handler.HasSingleTarget)
                throw new InvalidOperationException(SR.ObservableObservableDictionaryReentrancyNotAllowed);
        }
    }

    /// <summary>
    /// Helper to raise a PropertyChanged event for the Count property
    /// </summary>
    private void OnCountPropertyChanged() => OnPropertyChanged(EventArgsCache.CountPropertyChanged);

    /// <summary>
    /// Helper to raise a PropertyChanged event for the Indexer property
    /// </summary>
    private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

    /// <summary>
    /// Helper to raise CollectionChanged event to any listeners with an index-aware payload.
    /// Keeps an internal ordered key list and an index map in sync to provide stable indices.
    /// </summary>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> item)
    {

        switch (action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                // Append at the end to preserve insertion order.
                int index = _orderedKeys.Count;
                _orderedKeys.Add(item.Key);
                _indexMap[item.Key] = index;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
                break;
            }

            case NotifyCollectionChangedAction.Remove:
            {
                // Lookup current index from the map, then remove and fix tail indices.
                if (_indexMap.Remove(item.Key, out int index))
                {
                    _orderedKeys.RemoveAt(index);

                    // Update tail indices to preserve order (O(n) in the tail).
                    for (int j = index; j < _orderedKeys.Count; j++)
                        _indexMap[_orderedKeys[j]] = j;

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
                }
                else
                {
                    OnCollectionReset();
                }
                break;
            }

            default:
                Debug.Fail($"Unexpected action {action}");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
                break;
        }
    }

    /// <summary>
    /// Helper to raise CollectionChanged event to any listeners with an index-aware replace payload.
    /// </summary>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> oldItem, KeyValuePair<TKey, TValue> newItem)
    {
        if (action == NotifyCollectionChangedAction.Replace)
        {
            if (_indexMap.TryGetValue(oldItem.Key, out int index))
            {
                // Replace does not change order; the key is the same.
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
            }
            else
            {
                OnCollectionReset();
            }
        }
        else
        {
            Debug.Fail($"Unexpected action {action}");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
        }
    }

    /// <summary>
    /// Helper to raise CollectionChanged event with action == Reset to any listeners.
    /// Rebuilds the internal ordered key list and index map from the current dictionary state.
    /// </summary>
    private void OnCollectionReset()
    {
        _orderedKeys.Clear();
        _indexMap.Clear();
#if NET5_0_OR_GREATER
        _orderedKeys.EnsureCapacity(_innerDictionary.Count);
#endif
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        _indexMap.EnsureCapacity(_innerDictionary.Count);
#endif
        foreach (var kvp in _innerDictionary)
        {
            _indexMap[kvp.Key] = _orderedKeys.Count;
            _orderedKeys.Add(kvp.Key);
        }

        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }

    private SimpleMonitor EnsureMonitorInitialized() => _monitor ??= new SimpleMonitor(this);

    #endregion

    private sealed class SimpleMonitor(ObservableDictionary<TKey, TValue> collection) : IDisposable
    {
        public void Dispose() => collection._blockReentrancyCount--;
    }

    [Conditional("DEBUG")]
    private void ValidateState()
    {
        if (_orderedKeys.Count != _indexMap.Count || _orderedKeys.Count != _innerDictionary.Count)
            Debug.Fail("Counts mismatch");

        for (int i = 0; i < _orderedKeys.Count; i++)
        {
            var k = _orderedKeys[i];
            if (!_indexMap.TryGetValue(k, out int pos) || pos != i)
                Debug.Fail("Index map mismatch");

            if (!_innerDictionary.ContainsKey(k))
                Debug.Fail("Key present in order/index but missing in inner dictionary");
        }
    }

    #region IList implementation

    int IList.Add(object? value)
    {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        return -1;
    }

    void IList.Clear()
    {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
    }

    bool IList.Contains(object? value) => ((IList)this).IndexOf(value) >= 0;

    int IList.IndexOf(object? value)
    {
        if (value is KeyValuePair<TKey, TValue> kvp && ((ICollection<KeyValuePair<TKey, TValue>>)_innerDictionary).Contains(kvp)
                                                    && _indexMap.TryGetValue(kvp.Key, out int index))
        {
            return index;
        }
        return -1;
    }

    void IList.Insert(int index, object? value)
    {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
    }

    void IList.Remove(object? value)
    {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
    }

    void IList.RemoveAt(int index)
    {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
    }

    #endregion

}