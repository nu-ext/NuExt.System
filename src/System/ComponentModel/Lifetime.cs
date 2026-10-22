using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

// Based on Станислав Сидристый «Шаблон Lifetime: для сложного Disposing»
// https://www.youtube.com/watch?v=F5oOYKTFpcQ

namespace System.ComponentModel;

/// <summary>
/// Manages the synchronous lifecycle of resources.
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
public sealed class Lifetime : Disposable, ILifetime
{
    private readonly List<Action> _actions = [];
    private readonly Lock _syncLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Lifetime"/> class and sets up termination on disposal.
    /// </summary>
    public Lifetime()
    {
        Add(() => IsTerminated = true);
    }

    #region Properties

    /// <inheritdoc/>
    public bool IsTerminated { get; private set; }

    #endregion

    #region Methods

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Action action)
    {
        Debug.Assert(action != null, $"{nameof(action)} is null");
        ArgumentNullException.ThrowIfNull(action);

        CheckTerminatedOrDisposed();

        lock (_syncLock)
        {
            CheckTerminatedOrDisposed();
            _actions.Add(action);
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBracket(Action subscribe, Action unsubscribe)
    {
        Debug.Assert(subscribe != null, $"{nameof(subscribe)} is null");
        Debug.Assert(unsubscribe != null, $"{nameof(unsubscribe)} is null");
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);

        CheckTerminatedOrDisposed();

        lock (_syncLock)
        {
            CheckTerminatedOrDisposed();
            subscribe();
            _actions.Add(unsubscribe);
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
    /// Checks whether the <see cref="Lifetime"/> instance has been terminated or disposed and throws an exception if it has.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the <see cref="Lifetime"/> instance is terminated or disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckTerminatedOrDisposed()
    {
        ObjectDisposedException.ThrowIf(IsTerminated, this);
        CheckDisposingOrDisposed();
    }

    /// <summary>
    /// Executes all added actions in reverse order and marks the instance as terminated.
    /// Ensures that all resources are released properly.
    /// </summary>
    /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of added actions.</exception>
    protected override void DisposeCore()
    {
        List<Exception>? exceptions = null;
        var pool = ArrayPool<Action>.Shared;
        Action[] snapshot;
        int count;
        lock (_syncLock)
        {
            count = _actions.Count;
            snapshot = pool.Rent(count);
            _actions.CopyTo(snapshot);
            _actions.Clear();
        }
        for (int i = count - 1; i >= 0; i--)
        {
            try
            {
                snapshot[i]();
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>(i + 1);
                exceptions.Add(ex);
            }
        }

        Array.Clear(snapshot, 0, count);
        pool.Return(snapshot);

        Debug.Assert(IsTerminated, $"{nameof(Lifetime)} is not terminated");

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }

    #endregion
}