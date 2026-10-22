using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// Based on Станислав Сидристый «Шаблон Lifetime: для сложного Disposing»
// https://www.youtube.com/watch?v=F5oOYKTFpcQ

namespace System.ComponentModel;

/// <summary>
/// Manages the asynchronous lifecycle of resources.
/// Registered cleanup actions are executed upon disposal in LIFO (Last-In-First-Out) order.
/// </summary>
/// <remarks>
/// <para>
/// The LIFO execution order ensures that resources which are acquired later
/// (and may depend on earlier ones) are disposed of first, mirroring natural unwinding of nested dependencies.
/// </para>
/// <para>
/// For example, if you open a database connection and then begin a transaction,
/// the transaction should be disposed before the connection. By adding the connection disposal first
/// and the transaction disposal second, the LIFO order guarantees the correct sequence.
/// </para>
/// <para>
/// This class is thread-safe for concurrent modifications and disposal.
/// </para>
/// </remarks>
[DebuggerStepThrough]
public sealed class AsyncLifetime : AsyncDisposable, IAsyncLifetime
{
    private readonly List<(Func<ValueTask>?, Action?)> _actions = [];
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLifetime"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor initializes the <see cref="AsyncLifetime"/> instance with 
    /// <c>ContinueOnCapturedContext</c> set to <see langword="false"/>. By default, the current execution context 
    /// will not be captured to continue asynchronous operations in the <see cref="AsyncDisposable.DisposeAsync"/> method.
    /// </remarks>
    public AsyncLifetime()
    {
        Add(() => IsTerminated = true);
    }

    #region Properties

    /// <inheritdoc/>
    public bool IsTerminated { get; private set; }

    #endregion

    #region Methods

    /// <inheritdoc/>
    public void Add(Action action)
    {
        Debug.Assert(action != null, $"{nameof(action)} is null");
        ArgumentNullException.ThrowIfNull(action);

        CheckTerminatedOrDisposed();

        _syncLock.Wait();
        try
        {
            CheckTerminatedOrDisposed();
            _actions.Add((null, action));
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <inheritdoc/>
    public void AddAsync(Func<Task> callback)
    {
        Debug.Assert(callback != null, $"{nameof(callback)} is null");
        ArgumentNullException.ThrowIfNull(callback);

        CheckTerminatedOrDisposed();

        _syncLock.Wait();
        try
        {
            CheckTerminatedOrDisposed();
            _actions.Add((() => new ValueTask(callback()), null));
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <inheritdoc/>
    public void AddAsync(Func<ValueTask> callback)
    {
        Debug.Assert(callback != null, $"{nameof(callback)} is null");
        ArgumentNullException.ThrowIfNull(callback);

        CheckTerminatedOrDisposed();

        _syncLock.Wait();
        try
        {
            CheckTerminatedOrDisposed();
            _actions.Add((callback, null));
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T AddAsyncDisposable<T>(T disposable) where T : IAsyncDisposable
    {
        Debug.Assert(disposable != null, $"{nameof(disposable)} is null");
        ArgumentNullException.ThrowIfNull(disposable);

        AddAsync(disposable.DisposeAsync);
        return disposable;
    }

    /// <inheritdoc/>
    public void AddBracket(Action subscribe, Action unsubscribe)
    {
        Debug.Assert(subscribe != null, $"{nameof(subscribe)} is null");
        Debug.Assert(unsubscribe != null, $"{nameof(unsubscribe)} is null");
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);

        CheckTerminatedOrDisposed();

        _syncLock.Wait();
        try
        {
            CheckTerminatedOrDisposed();
            subscribe();
            _actions.Add((null, unsubscribe));
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask AddBracketAsync(Func<ValueTask> subscribe, Func<ValueTask> unsubscribe) =>
        AddBracketAsync(subscribe, unsubscribe, ContinueOnCapturedContext);

    /// <summary>
    /// Adds a pair of asynchronous actions: one to be executed immediately (subscribe) 
    /// and another to be executed during asynchronous disposal (unsubscribe).
    /// </summary>
    /// <param name="subscribe">The action to execute immediately.</param>
    /// <param name="unsubscribe">The action to execute upon asynchronous disposal.</param>
    /// <param name="subscribeOnCapturedContext">
    /// Determines whether the <paramref name="subscribe"/> callback begins execution
    /// on the captured synchronization context. When <see langword="true"/>, 
    /// the context is preserved for the start of <paramref name="subscribe"/>.
    /// </param>
    /// <remarks>
    /// <paramref name="subscribe"/> must not call methods on this <see cref="AsyncLifetime"/> instance,
    /// because the lifetime remains locked until the subscription completes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if either subscribe or unsubscribe is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if trying to add actions to a terminated <see cref="AsyncLifetime"/> instance.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async ValueTask AddBracketAsync(Func<ValueTask> subscribe, Func<ValueTask> unsubscribe, bool subscribeOnCapturedContext)
    {
        Debug.Assert(subscribe != null, $"{nameof(subscribe)} is null");
        Debug.Assert(unsubscribe != null, $"{nameof(unsubscribe)} is null");
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);

        CheckTerminatedOrDisposed();

        await _syncLock.WaitAsync().ConfigureAwait(subscribeOnCapturedContext);
        try
        {
            CheckTerminatedOrDisposed();
            await subscribe().ConfigureAwait(false);
            _actions.Add((unsubscribe, null));
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T AddDisposable<T>(T disposable) where T : IDisposable
    {
        Debug.Assert(disposable != null, $"{nameof(disposable)} is null");
        ArgumentNullException.ThrowIfNull(disposable);

        Add(disposable.Dispose);
        return disposable;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T AddRef<T>(T obj) where T : class
    {
        Debug.Assert(obj != null, $"{nameof(obj)} is null");
        ArgumentNullException.ThrowIfNull(obj);

        Add(() => GC.KeepAlive(obj));
        return obj;
    }

    /// <summary>
    /// Checks whether the <see cref="AsyncLifetime"/> instance has been terminated or disposed and throws an exception if it has.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the <see cref="AsyncLifetime"/> instance is terminated or disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckTerminatedOrDisposed()
    {
        ObjectDisposedException.ThrowIf(IsTerminated, this);
        CheckDisposingOrDisposed();
    }

    /// <summary>
    /// Executes all added asynchronous actions in reverse order and marks the instance as terminated.
    /// Ensures that all resources are released asynchronously.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of added asynchronous actions.</exception>
    protected override async ValueTask DisposeAsyncCore()
    {
        List<Exception>? exceptions = null;
        var pool = ArrayPool<(Func<ValueTask>?, Action?)>.Shared;
        (Func<ValueTask>?, Action?)[] snapshot;
        int count;
        await _syncLock.WaitAsync().ConfigureAwait(ContinueOnCapturedContext);
        try
        {
            count = _actions.Count;
            snapshot = pool.Rent(count);
            _actions.CopyTo(snapshot);
            _actions.Clear();
        }
        finally
        {
            _syncLock.Release();
        }

        for (int i = count - 1; i >= 0; i--)
        {
            var (callback, action) = snapshot[i];
            try
            {
                if (callback != null)
                {
                    await callback().ConfigureAwait(ContinueOnCapturedContext);
                    continue;
                }
                action?.Invoke();
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>(i + 1);
                exceptions.Add(ex);
            }
        }

        Array.Clear(snapshot, 0, count);
        pool.Return(snapshot);

        Debug.Assert(IsTerminated, $"{nameof(AsyncLifetime)} is not terminated");

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }

    #endregion
}