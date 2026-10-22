using System.Buffers;
using System.Collections.Generic;
using System.Threading;

namespace System.ComponentModel;

/// <summary>
/// Aggregates multiple disposable objects and disposes them synchronously as a single unit.
/// </summary>
/// <remarks>
/// This class is thread-safe for concurrent modifications and disposal.
/// </remarks>
public sealed class AggregateDisposable : Disposable
{
    /// <summary>
    /// The list of disposables to be managed and disposed together.
    /// </summary>
    private readonly List<IDisposable?> _disposables;

    private readonly Lock _syncLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables and synchronization context.
    /// </summary>
    /// <param name="disposables">A collection of disposables to aggregate.</param>
    /// <param name="synchronizationContext">An optional <see cref="SynchronizationContext"/> to use for property change notifications.</param>
    /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
    public AggregateDisposable(IEnumerable<IDisposable?> disposables, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
    {
        _ = disposables ?? throw new ArgumentNullException(nameof(disposables));
        _disposables = [.. disposables];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables.
    /// </summary>
    /// <param name="disposables">A collection of disposables to aggregate.</param>
    /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
    public AggregateDisposable(IEnumerable<IDisposable?> disposables) : this(disposables, null)
    {
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables.
    /// </summary>
    /// <param name="disposables">A read-only span of disposables to aggregate.</param>
    public AggregateDisposable(params ReadOnlySpan<IDisposable?> disposables)
    {
        _disposables = [.. disposables];
    }
#else
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables.
    /// </summary>
    /// <param name="disposables">An array of disposables to aggregate.</param>
    /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
    public AggregateDisposable(params IDisposable?[] disposables) : this(disposables, null)
    {
    }
#endif

    /// <summary>
    /// Adds a disposable object to the aggregate.
    /// </summary>
    /// <param name="disposable">The disposable object to add.</param>
    public void Add(IDisposable? disposable)
    {
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            _disposables.Add(disposable);
        }
    }

    /// <summary>
    /// Adds a range of disposable objects to the aggregate.
    /// </summary>
    /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
    public void AddRange(IEnumerable<IDisposable?> disposables)
    {
        ArgumentNullException.ThrowIfNull(disposables);
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            _disposables.AddRange(disposables);
        }
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Adds a range of disposable objects to the aggregate from a read-only span.
    /// </summary>
    /// <param name="disposables">A read-only span of disposable objects to add.</param>
    public void AddRange(params ReadOnlySpan<IDisposable?> disposables)
    {
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            foreach (var disposable in disposables)
            {
                _disposables.Add(disposable);
            }
        }
    }
#else
    /// <summary>
    /// Adds a range of disposable objects to the aggregate.
    /// </summary>
    /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
    public void AddRange(params IDisposable?[] disposables)
    {
        ArgumentNullException.ThrowIfNull(disposables);
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            _disposables.AddRange(disposables);
        }
    }
#endif

    /// <summary>
    /// Disposes all aggregated disposables.
    /// </summary>
    /// <exception cref="AggregateException">
    /// Thrown if one or more exceptions occur during the disposal of the aggregated disposables.
    /// </exception>
    protected override void DisposeCore()
    {
        List<Exception>? exceptions = null;
        var pool = ArrayPool<IDisposable?>.Shared;
        IDisposable?[] snapshot;
        int count;
        lock (_syncLock)
        {
            count = _disposables.Count;
            snapshot = pool.Rent(count);
            _disposables.CopyTo(snapshot);
            _disposables.Clear();
        }

        for (int i = 0; i < count; i++)
        {
            var disposable = snapshot[i];
            try
            {
                disposable?.Dispose();
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }

        Array.Clear(snapshot, 0, count);
        pool.Return(snapshot);

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }
}
