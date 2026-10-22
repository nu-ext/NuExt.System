using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NuExt.System.Tests.Collections;

[TestFixture]
public class ObservableDictionaryTests
{
    private ObservableDictionary<int, string> _dict;
    private EventRecorder _rec;

    [SetUp]
    public void SetUp()
    {
        _dict = [];
        _rec = new EventRecorder();
        _dict.CollectionChanged += _rec.OnCollectionChanged;
        _dict.PropertyChanged += _rec.OnPropertyChanged;
    }

    [TearDown]
    public void TearDown()
    {
        _dict.CollectionChanged -= _rec.OnCollectionChanged;
        _dict.PropertyChanged -= _rec.OnPropertyChanged;
    }

    // -------------------------
    // ORDERING & ENUMERATION
    // -------------------------

    [Test]
    public void Foreach_and_IList_Indexer_have_same_order()
    {
        _dict.Add(1, "a");
        _dict.Add(3, "c");
        _dict.AddRange([
            new KeyValuePair<int,string>(2,"b"),
            new KeyValuePair<int,string>(4,"d")
        ]);

        var viaForeach = _dict.ToArray();
        var ilist = (IList)_dict;
        var viaIndex = Enumerable.Range(0, _dict.Count)
                                 .Select(i => (KeyValuePair<int, string>)ilist[i]!)
                                 .ToArray();

        Assert.That(viaIndex, Is.EqualTo(viaForeach), "IList indexer and foreach order must match");
        Assert.That(viaIndex.Select(kv => kv.Key), Is.EqualTo([1, 3, 2, 4]));
    }

    [Test]
    public void CopyTo_generic_and_nongeneric_preserve_order()
    {
        _dict.AddRange([
            new KeyValuePair<int,string>(1,"a"),
            new KeyValuePair<int,string>(2,"b"),
            new KeyValuePair<int,string>(3,"c")
        ]);

        // generic
        var arr1 = new KeyValuePair<int, string>[_dict.Count];
        ((ICollection<KeyValuePair<int, string>>)_dict).CopyTo(arr1, 0);

        // non-generic
        var arr2 = new object[_dict.Count];
        ((ICollection)_dict).CopyTo(arr2, 0);

        var seq2 = arr2.Cast<KeyValuePair<int, string>>().ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(arr1, Is.EqualTo(_dict.ToArray()));
            Assert.That(seq2, Is.EqualTo(_dict.ToArray()));
        }
    }

    // -------------------------
    // SINGLE OPERATIONS
    // -------------------------

    [Test]
    public void Add_raises_Add_with_index_at_tail_and_updates_properties()
    {
        _dict.Add(1, "a"); _rec.Reset();
        _dict.Add(2, "b");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_dict, Has.Count.EqualTo(2));

            // PropertyChanged
            Assert.That(_rec.PropertyNames, Does.Contain("Count"));
        }
        Assert.That(_rec.PropertyNames, Does.Contain("Item[]"));

        // INCC
        var e = _rec.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(e.NewStartingIndex, Is.EqualTo(1));
            Assert.That(((KeyValuePair<int, string>)e.NewItems![0]!).Key, Is.EqualTo(2));
            Assert.That(((KeyValuePair<int, string>)e.NewItems[0]!).Value, Is.EqualTo("b"));
        }
    }

    [Test]
    public void Remove_raises_Remove_with_correct_index_and_updates_properties()
    {
        _dict.AddRange([
            new KeyValuePair<int,string>(1,"a"),
            new KeyValuePair<int,string>(2,"b"),
            new KeyValuePair<int,string>(3,"c")
        ]);
        _rec.Reset();

        var removed = _dict.Remove(2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(removed, Is.True);
            Assert.That(_dict, Has.Count.EqualTo(2));

            Assert.That(_rec.PropertyNames, Does.Contain("Count"));
        }
        Assert.That(_rec.PropertyNames, Does.Contain("Item[]"));

        var e = _rec.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(e.OldStartingIndex, Is.EqualTo(1));
            Assert.That(((KeyValuePair<int, string>)e.OldItems![0]!).Key, Is.EqualTo(2));
        }
    }

    [Test]
    public void Replace_via_indexer_raises_Replace_at_same_index()
    {
        _dict.AddRange([
            new KeyValuePair<int,string>(1,"a"),
            new KeyValuePair<int,string>(2,"b")
        ]);
        _rec.Reset();

        _dict[2] = "b2";

        // PropertyChanged
        Assert.That(_rec.PropertyNames, Does.Contain("Item[]"));

        // INCC
        var e = _rec.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
            Assert.That(e.NewStartingIndex, Is.EqualTo(1));
        }
        var oldItem = (KeyValuePair<int, string>)e.OldItems![0]!;
        var newItem = (KeyValuePair<int, string>)e.NewItems![0]!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(oldItem.Key, Is.EqualTo(2));
            Assert.That(oldItem.Value, Is.EqualTo("b"));
            Assert.That(newItem.Key, Is.EqualTo(2));
            Assert.That(newItem.Value, Is.EqualTo("b2"));
        }
    }

    // -------------------------
    // BATCH OPERATIONS
    // -------------------------

    [Test]
    public void AddRange_multiple_raises_single_Reset_and_appends_in_input_order()
    {
        _dict.Add(10, "x");
        _rec.Reset();

        _dict.AddRange([
            new KeyValuePair<int,string>(1,"a"),
            new KeyValuePair<int,string>(2,"b"),
            new KeyValuePair<int,string>(3,"c")
        ]);


        Assert.That(_rec.Count, Is.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_rec[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));

            Assert.That(_dict.Select(kv => kv.Key), Is.EqualTo([10, 1, 2, 3]));
        }

        var ilist = (IList)_dict;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(10, "x")), Is.Zero);
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(1, "a")), Is.EqualTo(1));
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(2, "b")), Is.EqualTo(2));
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(3, "c")), Is.EqualTo(3));
        }
    }

    [Test]
    public void RemoveRange_multiple_raises_single_Reset_and_compacts_indices()
    {
        _dict.AddRange([
            new KeyValuePair<int,string>(1,"a"),
            new KeyValuePair<int,string>(2,"b"),
            new KeyValuePair<int,string>(3,"c"),
            new KeyValuePair<int,string>(4,"d")
        ]);
        _rec.Reset();

        var removed = _dict.RemoveRange([2, 4]);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(removed.Select(kv => kv.Key), Is.EquivalentTo([2, 4]));

            Assert.That(_rec.Count, Is.EqualTo(1));
        }
        Assert.That(_rec[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));

        var ilist = (IList)_dict;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_dict.Select(kv => kv.Key), Is.EqualTo([1, 3]));
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(1, "a")), Is.Zero);
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(3, "c")), Is.EqualTo(1));
        }
    }

    // -------------------------
    // IList behavior
    // -------------------------

    [Test]
    public void IList_is_readonly_and_throws_on_mutations()
    {
        _dict.Add(1, "a");
        var ilist = (IList)_dict;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ilist.IsReadOnly, Is.True);
            Assert.That(ilist.IsFixedSize, Is.True);
        }

        Assert.Throws<NotSupportedException>(() => ilist.Add(new KeyValuePair<int, string>(2, "b")));
        Assert.Throws<NotSupportedException>(() => ilist.Insert(0, new KeyValuePair<int, string>(2, "b")));
        Assert.Throws<NotSupportedException>(() => ilist.Remove(new KeyValuePair<int, string>(1, "a")));
        Assert.Throws<NotSupportedException>(() => ilist.RemoveAt(0));

        var item0 = (KeyValuePair<int, string>)ilist[0]!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(item0.Key, Is.EqualTo(1));
            Assert.That(item0.Value, Is.EqualTo("a"));
        }
    }

    [Test]
    public void IList_IndexOf_uses_key_semantics()
    {
        _dict.AddRange([
            new KeyValuePair<int,string>(1,"a"),
            new KeyValuePair<int,string>(2,"b")
        ]);
        var ilist = (IList)_dict;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(1, "a")), Is.Zero);
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(2, "b")), Is.EqualTo(1));
            Assert.That(ilist.IndexOf(new KeyValuePair<int, string>(3, "c")), Is.EqualTo(-1));
        }
    }

    // -------------------------
    // PropertyChanged on Count / Item[]
    // -------------------------

    [Test]
    public void PropertyChanged_raised_for_Count_and_Indexer()
    {
        _dict.Add(1, "a");
        _rec.Reset();

        _dict.Add(2, "b");
        Assert.That(_rec.PropertyNames, Does.Contain("Count"));
        Assert.That(_rec.PropertyNames, Does.Contain("Item[]"));

        _rec.Reset();
        _dict[1] = "a2";
        Assert.That(_rec.PropertyNames, Does.Contain("Item[]"));
    }

    // -------------------------
    // Clear
    // -------------------------

    [Test]
    public void Clear_raises_Reset_and_collection_becomes_empty()
    {
        _dict.AddRange([
            new KeyValuePair<int,string>(1,"a"),
            new KeyValuePair<int,string>(2,"b")
        ]);
        _rec.Reset();

        _dict.Clear();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_dict, Is.Empty);
            Assert.That(_rec.Count, Is.EqualTo(1));
        }
        Assert.That(_rec[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
    }

    // -------------------------
    // Helpers
    // -------------------------

    private sealed class EventRecorder
    {
        private readonly List<NotifyCollectionChangedEventArgs> _events = [];
        private readonly HashSet<string> _propNames = new(StringComparer.Ordinal);

        public void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => _events.Add(e);

        public void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.PropertyName))
                _propNames.Add(e.PropertyName!);
        }

        public void Reset()
        {
            _events.Clear();
            _propNames.Clear();
        }

        public int Count => _events.Count;

        public NotifyCollectionChangedEventArgs this[int index] => _events[index];

        public IReadOnlyCollection<string> PropertyNames => _propNames;

        public NotifyCollectionChangedEventArgs Single()
        {
            Assert.That(_events, Has.Count.EqualTo(1), "Expected exactly one INCC event");
            return _events[0];
        }
    }
}
