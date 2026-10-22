using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.ComponentModel;

/// <summary>
/// Aggregates multiple disposable objects and disposes them asynchronously as a single unit.
/// </summary>
/// <remarks>
/// This class is thread-safe for concurrent modifications and disposal.
/// </remarks>
public sealed class AggregateAsyncDisposable : AsyncDisposable
{
    /// <summary>
    /// The list of disposables to be managed and disposed together asynchronously.
    /// </summary>
    private readonly List<(IAsyncDisposable?, IDisposable?)> _disposables;

    private readonly Lock _syncLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables and synchronization context.
    /// </summary>
    /// <param name="disposables">A collection of async disposables to aggregate.</param>
    /// <param name="synchronizationContext">An optional <see cref="SynchronizationContext"/> to use for property change notifications.</param>
    /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
    public AggregateAsyncDisposable(IEnumerable<IAsyncDisposable?> disposables, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
    {
        _ = disposables ?? throw new ArgumentNullException(nameof(disposables));
        List<(IAsyncDisposable?, IDisposable?)> list = [];
        foreach (var disposable in disposables)
        {
            list.Add((disposable, null));
        }
        _disposables = list;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables.
    /// </summary>
    /// <param name="disposables">A collection of disposables to aggregate.</param>
    /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
    public AggregateAsyncDisposable(IEnumerable<IAsyncDisposable?> disposables) : this(disposables, null)
    {
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables.
    /// </summary>
    /// <param name="disposables">A read-only span of disposables to aggregate.</param>
    public AggregateAsyncDisposable(params ReadOnlySpan<IAsyncDisposable?> disposables)
    {
        List<(IAsyncDisposable?, IDisposable?)> list = new(disposables.Length);
        foreach (var disposable in disposables)
        {
            list.Add((disposable, null));
        }
        _disposables = list;
    }
#else
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables.
    /// </summary>
    /// <param name="disposables">An array of disposables to aggregate.</param>
    /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
    public AggregateAsyncDisposable(params IAsyncDisposable?[] disposables) : this(disposables, null)
    {
    }
#endif

    /// <summary>
    /// Adds a disposable object to the aggregate.
    /// </summary>
    /// <param name="disposable">The disposable object to add.</param>
    public void Add(IAsyncDisposable? disposable)
    {
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            _disposables.Add((disposable, null));
        }
    }

    /// <summary>
    /// Adds a disposable object to the aggregate.
    /// </summary>
    /// <param name="disposable">The disposable object to add.</param>
    [OverloadResolutionPriority(-1)]
    public void Add(IDisposable? disposable)
    {
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            if (disposable is IAsyncDisposable asyncDisposable)
            {
                _disposables.Add((asyncDisposable, null));
            }
            else
            {
                _disposables.Add((null, disposable));
            }
        }
    }

    /// <summary>
    /// Adds a range of disposable objects to the aggregate.
    /// </summary>
    /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
    public void AddRange(IEnumerable<IAsyncDisposable?> disposables)
    {
        ArgumentNullException.ThrowIfNull(disposables);
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            foreach (var disposable in disposables)
            {
                _disposables.Add((disposable, null));
            }
        }
    }

    /// <summary>
    /// Adds a range of disposable objects to the aggregate.
    /// </summary>
    /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
    [OverloadResolutionPriority(-1)]
    public void AddRange(IEnumerable<IDisposable?> disposables)
    {
        ArgumentNullException.ThrowIfNull(disposables);
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            foreach (var disposable in disposables)
            {
                if (disposable is IAsyncDisposable asyncDisposable)
                {
                    _disposables.Add((asyncDisposable, null));
                }
                else
                {
                    _disposables.Add((null, disposable));
                }
            }
        }
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Adds a range of disposable objects to the aggregate from a read-only span.
    /// </summary>
    /// <param name="disposables">A read-only span of disposable objects to add.</param>
    public void AddRange(params ReadOnlySpan<IAsyncDisposable?> disposables)
    {
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            foreach (var disposable in disposables)
            {
                _disposables.Add((disposable, null));
            }
        }
    }

    /// <summary>
    /// Adds a range of disposable objects to the aggregate from a read-only span.
    /// </summary>
    /// <param name="disposables">A read-only span of disposable objects to add.</param>
    [OverloadResolutionPriority(-1)]
    public void AddRange(params ReadOnlySpan<IDisposable?> disposables)
    {
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            foreach (var disposable in disposables)
            {
                if (disposable is IAsyncDisposable asyncDisposable)
                {
                    _disposables.Add((asyncDisposable, null));
                }
                else
                {
                    _disposables.Add((null, disposable));
                }
            }
        }
    }
#else
    /// <summary>
    /// Adds a range of disposable objects to the aggregate.
    /// </summary>
    /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
    public void AddRange(params IAsyncDisposable?[] disposables)
    {
        ArgumentNullException.ThrowIfNull(disposables);
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            foreach (var disposable in disposables)
            {
                _disposables.Add((disposable, null));
            }
        }
    }

    /// <summary>
    /// Adds a range of disposable objects to the aggregate.
    /// </summary>
    /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
    [OverloadResolutionPriority(-1)]
    public void AddRange(params IDisposable?[] disposables)
    {
        ArgumentNullException.ThrowIfNull(disposables);
        CheckDisposingOrDisposed();

        lock (_syncLock)
        {
            CheckDisposingOrDisposed();
            foreach (var disposable in disposables)
            {
                if (disposable is IAsyncDisposable asyncDisposable)
                {
                    _disposables.Add((asyncDisposable, null));
                }
                else
                {
                    _disposables.Add((null, disposable));
                }
            }
        }
    }
#endif

    /// <summary>
    /// Disposes all aggregated disposables asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    /// <exception cref="AggregateException">Thrown if one or more exceptions occur during the disposal of the aggregated disposables.</exception>
    protected override async ValueTask DisposeAsyncCore()
    {
        List<Exception>? exceptions = null;
        var pool = ArrayPool<(IAsyncDisposable?, IDisposable?)>.Shared;
        (IAsyncDisposable?, IDisposable?)[] snapshot;
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
            var (asyncDisposable, disposable) = snapshot[i];
            try
            {
                if (asyncDisposable != null)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(continueOnCapturedContext: ContinueOnCapturedContext);
                    continue;
                }
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
