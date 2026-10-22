using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Threading;

/// <summary>
/// Provides synchronous and asynchronous mutual exclusion with reentrancy within the same execution flow.
/// The owning execution flow can enter the lock recursively without deadlocking.
/// </summary>
/// <remarks>
/// <para>
/// Reentrancy follows the ambient asynchronous execution context. Nested operations that acquire the lock
/// must remain within the current acquisition scope and must complete before that scope exits.
/// </para>
/// <para>
/// Parallel branches that inherit the owning execution context are treated as reentrant. Mutual exclusion
/// is therefore not provided between those branches; callers must not access the same protected state
/// concurrently from multiple branches.
/// </para>
/// <para>
/// Dispose this instance only after all acquisition scopes and pending acquisition operations have completed.
/// Disposal is not synchronized with active or pending operations.
/// </para>
/// </remarks>
public sealed partial class ReentrantAsyncLock : Disposable
{
    /// <summary>
    /// A static counter used to generate non-zero identifiers for execution flows.
    /// </summary>
    private static int s_idCounter;

    /// <summary>
    /// Semaphore to ensure mutual exclusion for synchronous and asynchronous operations.
    /// </summary>
    /// <remarks>
    /// Initialized with a count of 1, it allows only one concurrent access at a time,
    /// serving as the primary locking mechanism for this class.
    /// </remarks>
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    /// An async-local identifier associated with the current execution flow.
    private readonly AsyncLocal<int> _localId = new();

    /// <summary>
    /// An async-local synchronization root used for nested lock acquisition within the same execution flow.
    /// </summary>
    private readonly AsyncLocal<SemaphoreSlim?> _localSyncRoot = new();

    /// <summary>
    /// The entry depth of the execution flow that currently owns the lock.
    /// </summary>
    private int _currentCount;

    /// The identifier of the execution flow that currently owns the lock.
    private volatile int _currentId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReentrantAsyncLock"/> class 
    /// without a synchronization context.
    /// </summary>
    public ReentrantAsyncLock()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReentrantAsyncLock"/> class 
    /// with the specified synchronization context.
    /// </summary>
    /// <param name="synchronizationContext">
    /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
    /// If null, no synchronization context will be used.
    /// </param>
    public ReentrantAsyncLock(SynchronizationContext? synchronizationContext) : base(synchronizationContext) { }

    #region Properties

    /// <summary>
    /// Gets the current reentrant entry count for the lock.
    /// </summary>
    /// <remarks>
    /// The value represents the entry depth of the execution flow that currently owns the lock.
    /// It is zero when the lock is not held. This property is intended for debugging and diagnostics.
    /// </remarks>
    public int CurrentCount => _currentCount;

    /// <summary>
    /// Gets the identifier of the execution flow that currently owns the lock.
    /// </summary>
    /// <remarks>
    /// The value is zero when the lock is not held. This property is intended for internal diagnostics and synchronization validation.
    /// </remarks>
    internal int CurrentId => _currentId;

    /// <summary>
    /// Gets a value that indicates whether the lock is held by the current execution flow.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the current execution flow holds the lock;
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property provides functionality analogous to <see cref="Monitor.IsEntered(object)"/> 
    /// for asynchronous, reentrant locks. The check is non-blocking and reflects the lock state
    /// at the precise moment of the call.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The returned value represents a momentary snapshot that may
    /// be obsolete immediately after retrieval. It must not be used to control program flow
    /// for lock acquisition or release, as doing so will lead to race conditions.
    /// For thread-safe lock operations, use the <see cref="Acquire(Action, CancellationToken)"/> or similar methods.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="ReentrantAsyncLock"/> has been disposed.
    /// </exception>
    public bool IsHeldByCurrentFlow
    {
        get
        {
            CheckDisposed();
            int localId = _localId.Value;
            return localId != 0 && _currentId == localId && _syncLock.CurrentCount == 0;
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the lock is currently held by any execution flow.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the lock is currently held by any execution flow;
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property checks the lock's occupancy status, regardless of which specific flow holds it.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> This property returns a momentary state snapshot
    /// which may be obsolete upon return. It cannot identify the specific flow holding
    /// the lock. Using this property to guide synchronization logic will result in
    /// race conditions and compromise the mutual exclusion guarantee.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="ReentrantAsyncLock"/> has been disposed.
    /// </exception>
    public bool IsLocked
    {
        get
        {
            CheckDisposed();
            return _syncLock.CurrentCount == 0;
        }
    }

    /// <summary>
    /// Gets the ambient local identifier for the current instance.
    /// </summary>
    /// <remarks>
    /// If the identifier has not been set, it generates a new identifier using the <see cref="NewId"/> method.
    /// This non-zero identifier is associated with the current execution flow and is used to manage reentrant lock acquisition.
    /// Primarily intended for internal use.
    /// </remarks>
    internal int LocalId => _localId.Value != 0 ? _localId.Value : (_localId.Value = NewId());

    /// <summary>
    /// Gets or sets the async-flow-local synchronization root for nested lock acquisition.
    /// </summary>
    /// <remarks>
    /// This property provides a <see cref="SemaphoreSlim"/> associated with the current asynchronous execution flow.
    /// The value flows across asynchronous continuations through <see cref="AsyncLocal{T}"/> and is intended for internal use only.
    /// </remarks>
    internal SemaphoreSlim? SyncRoot
    {
        get => _localSyncRoot.Value;
        private set => _localSyncRoot.Value = value;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Acquires the lock for the current execution flow, executes the provided action, and then releases the lock.
    /// </summary>
    /// <param name="action">
    /// The <see cref="Action"/> to be executed while holding the lock. Must not be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="action"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="SynchronizationLockException">
    /// Thrown if there is an inconsistency in the synchronization state.
    /// </exception>
    /// <remarks>
    /// The method ensures thread-safe execution of the provided action by acquiring either an exclusive
    /// or a nested (reentrant) lock depending on the calling execution flow. For reentrant locks,
    /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
    /// </remarks>
    public void Acquire(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        CheckDisposed();

        bool isReentrant = LocalId == CurrentId;
        SemaphoreSlim? previousSyncRoot = null;
        if (isReentrant)//Reentrant lock in the flow
        {
            ValidateReentrantState();

            previousSyncRoot = SyncRoot;
            if (previousSyncRoot == null)
            {
                ThrowSynchronizationLockException();
            }
            previousSyncRoot!.Wait(cancellationToken);//Acquires a nested lock in the flow
        }
        else
        {
            _syncLock.Wait(cancellationToken);//Acquires an exclusive lock in the flow

            ValidateExclusiveState();

            _currentId = LocalId;//The lock is obtained
        }

        Interlocked.Increment(ref _currentCount);

        using SemaphoreSlim localSyncRoot = new(1, 1);
        SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock

        try
        {
            action();
        }
        finally
        {
            PerformValidationAfterExecution(isReentrant, localSyncRoot);

            SyncRoot = previousSyncRoot;// Restore the previous synchronization root to maintain correct synchronization state and nested lock handling in the current flow.

            // Do not release the underlying semaphore if ownership validation fails.
            // The lock state is no longer considered reliable.
            CheckSynchronizationLock();

            if (Interlocked.Decrement(ref _currentCount) == 0)
            {
                ValidateExclusiveStateFinally(isReentrant, previousSyncRoot);
                _currentId = 0;//The lock is released
                _syncLock.Release();//Releases an exclusive lock
            }
            else
            {
                ValidateReentrantStateFinally(isReentrant, previousSyncRoot);
                previousSyncRoot!.Release();//Releases a nested lock
            }
        }
    }

    /// <summary>
    /// Acquires the lock for the current execution flow, executes the provided action, and then releases the lock.
    /// </summary>
    /// <typeparam name="TState">
    /// The type of the state passed to the action.
    /// </typeparam>
    /// <param name="action">
    /// The action to be executed while holding the lock. Must not be null.
    /// </param>
    /// <param name="state">
    /// The state to pass to the action.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="action"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="SynchronizationLockException">
    /// Thrown if there is an inconsistency in the synchronization state.
    /// </exception>
    /// <remarks>
    /// The method ensures thread-safe execution of the provided action by acquiring either an exclusive
    /// or a nested (reentrant) lock depending on the calling execution flow. For reentrant locks,
    /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
    /// </remarks>
    public void Acquire<TState>(Action<TState> action, TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        CheckDisposed();
        bool isReentrant = LocalId == CurrentId;
        SemaphoreSlim? previousSyncRoot = null;
        if (isReentrant)//Reentrant lock in the flow
        {
            ValidateReentrantState();
            previousSyncRoot = SyncRoot;
            if (previousSyncRoot == null)
            {
                ThrowSynchronizationLockException();
            }
            previousSyncRoot!.Wait(cancellationToken);//Acquires a nested lock in the flow
        }
        else
        {
            _syncLock.Wait(cancellationToken);//Acquires an exclusive lock in the flow
            ValidateExclusiveState();
            _currentId = LocalId;//The lock is obtained
        }
        Interlocked.Increment(ref _currentCount);
        using SemaphoreSlim localSyncRoot = new(1, 1);
        SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock
        try
        {
            action(state);
        }
        finally
        {
            PerformValidationAfterExecution(isReentrant, localSyncRoot);

            SyncRoot = previousSyncRoot;// Restore the previous synchronization root to maintain correct synchronization state and nested lock handling in the current flow.

            // Do not release the underlying semaphore if ownership validation fails.
            // The lock state is no longer considered reliable.
            CheckSynchronizationLock();

            if (Interlocked.Decrement(ref _currentCount) == 0)
            {
                ValidateExclusiveStateFinally(isReentrant, previousSyncRoot);
                _currentId = 0;//The lock is released
                _syncLock.Release();//Releases an exclusive lock
            }
            else
            {
                ValidateReentrantStateFinally(isReentrant, previousSyncRoot);
                previousSyncRoot!.Release();//Releases a nested lock
            }
        }
    }

    /// <summary>
    /// Acquires the lock for the current execution flow, executes the provided action, and then releases the lock.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the result produced by the function.
    /// </typeparam>
    /// <param name="func">
    /// The <see cref="Func{T}"/> to be executed while holding the lock. Must not be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// The result of the execution of the provided function.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="func"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="SynchronizationLockException">
    /// Thrown if there is an inconsistency in the synchronization state.
    /// </exception>
    /// <remarks>
    /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
    /// or a nested (reentrant) lock depending on the calling execution flow. For reentrant locks,
    /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
    /// </remarks>
    public T Acquire<T>(Func<T> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        CheckDisposed();

        bool isReentrant = LocalId == CurrentId;
        SemaphoreSlim? previousSyncRoot = null;
        if (isReentrant)//Reentrant lock in the flow
        {
            ValidateReentrantState();

            previousSyncRoot = SyncRoot;
            if (previousSyncRoot == null)
            {
                ThrowSynchronizationLockException();
            }
            previousSyncRoot!.Wait(cancellationToken);//Acquires a nested lock in the flow
        }
        else
        {
            _syncLock.Wait(cancellationToken);//Acquires an exclusive lock in the flow

            ValidateExclusiveState();

            _currentId = LocalId;//The lock is obtained
        }

        Interlocked.Increment(ref _currentCount);

        using SemaphoreSlim localSyncRoot = new(1, 1);
        SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock

        try
        {
            return func();
        }
        finally
        {
            PerformValidationAfterExecution(isReentrant, localSyncRoot);

            SyncRoot = previousSyncRoot;// Restore the previous synchronization root to maintain correct synchronization state and nested lock handling in the current flow.

            // Do not release the underlying semaphore if ownership validation fails.
            // The lock state is no longer considered reliable.
            CheckSynchronizationLock();

            if (Interlocked.Decrement(ref _currentCount) == 0)
            {
                ValidateExclusiveStateFinally(isReentrant, previousSyncRoot);
                _currentId = 0;//The lock is released
                _syncLock.Release();//Releases an exclusive lock
            }
            else
            {
                ValidateReentrantStateFinally(isReentrant, previousSyncRoot);
                previousSyncRoot!.Release();//Releases a nested lock
            }
        }
    }

    /// <summary>
    /// Acquires the lock for the current execution flow, executes the provided action, and then releases the lock.
    /// </summary>
    /// <typeparam name="TState">
    /// The type of the state passed to the function.
    /// </typeparam>
    /// <typeparam name="TResult">
    /// The type of the result produced by the function.
    /// </typeparam>
    /// <param name="func">
    /// The function to be executed while holding the lock. Must not be null.
    /// </param>
    /// <param name="state">
    /// The state to pass to the function.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// The result of the execution of the provided function.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="func"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="SynchronizationLockException">
    /// Thrown if there is an inconsistency in the synchronization state.
    /// </exception>
    /// <remarks>
    /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
    /// or a nested (reentrant) lock depending on the calling execution flow. For reentrant locks,
    /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
    /// </remarks>
    public TResult Acquire<TState, TResult>(Func<TState, TResult> func, TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        CheckDisposed();
        bool isReentrant = LocalId == CurrentId;
        SemaphoreSlim? previousSyncRoot = null;
        if (isReentrant)//Reentrant lock in the flow
        {
            ValidateReentrantState();
            previousSyncRoot = SyncRoot;
            if (previousSyncRoot == null)
            {
                ThrowSynchronizationLockException();
            }
            previousSyncRoot!.Wait(cancellationToken);//Acquires a nested lock in the flow
        }
        else
        {
            _syncLock.Wait(cancellationToken);//Acquires an exclusive lock in the flow
            ValidateExclusiveState();
            _currentId = LocalId;//The lock is obtained
        }
        Interlocked.Increment(ref _currentCount);
        using SemaphoreSlim localSyncRoot = new(1, 1);
        SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock
        try
        {
            return func(state);
        }
        finally
        {
            PerformValidationAfterExecution(isReentrant, localSyncRoot);

            SyncRoot = previousSyncRoot;// Restore the previous synchronization root to maintain correct synchronization state and nested lock handling in the current flow.

            // Do not release the underlying semaphore if ownership validation fails.
            // The lock state is no longer considered reliable.
            CheckSynchronizationLock();

            if (Interlocked.Decrement(ref _currentCount) == 0)
            {
                ValidateExclusiveStateFinally(isReentrant, previousSyncRoot);
                _currentId = 0;//The lock is released
                _syncLock.Release();//Releases an exclusive lock
            }
            else
            {
                ValidateReentrantStateFinally(isReentrant, previousSyncRoot);
                previousSyncRoot!.Release();//Releases a nested lock
            }
        }
    }

    /// <summary>
    /// Asynchronously acquires the lock for the current execution flow, executes the provided asynchronous function, and then releases the lock.
    /// </summary>
    /// <param name="func">
    /// The <see cref="Func{ValueTask}"/> to be executed while holding the lock. Must not be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="func"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="SynchronizationLockException">
    /// Thrown if there is an inconsistency in the synchronization state.
    /// </exception>
    /// <remarks>
    /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
    /// or a nested (reentrant) lock depending on the calling execution flow. For reentrant locks,
    /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
    /// </remarks>
    public async ValueTask AcquireAsync(Func<ValueTask> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        CheckDisposed();

        bool isReentrant = LocalId == CurrentId;
        SemaphoreSlim? previousSyncRoot = null;
        if (isReentrant)//Reentrant lock in the flow
        {
            ValidateReentrantState();

            previousSyncRoot = SyncRoot;
            if (previousSyncRoot == null)
            {
                ThrowSynchronizationLockException();
            }
            await previousSyncRoot!.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires a nested lock in the flow
        }
        else
        {
            await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires an exclusive lock in the flow

            ValidateExclusiveState();

            _currentId = LocalId;//The lock is obtained
        }

        Interlocked.Increment(ref _currentCount);

        using SemaphoreSlim localSyncRoot = new(1, 1);
        SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock

        try
        {
            await func().ConfigureAwait(false);
        }
        finally
        {
            PerformValidationAfterExecution(isReentrant, localSyncRoot);

            // Clear this async flow's synchronization root. The caller's AsyncLocal value is preserved by its ExecutionContext.
            SyncRoot = null;

            // Do not release the underlying semaphore if ownership validation fails.
            // The lock state is no longer considered reliable.
            CheckSynchronizationLock();

            if (Interlocked.Decrement(ref _currentCount) == 0)
            {
                ValidateExclusiveStateFinally(isReentrant, previousSyncRoot);
                _currentId = 0;//The lock is released
                _syncLock.Release();//Releases an exclusive lock
            }
            else
            {
                ValidateReentrantStateFinally(isReentrant, previousSyncRoot);
                previousSyncRoot!.Release();//Releases a nested lock
            }
        }
    }

    /// <summary>
    /// Asynchronously acquires the lock for the current execution flow, executes the provided asynchronous function, and then releases the lock.
    /// </summary>
    /// <typeparam name="TState">
    /// The type of the state passed to the function.
    /// </typeparam>
    /// <param name="func">
    /// The asynchronous function to be executed while holding the lock. Must not be null.
    /// </param>
    /// <param name="state">
    /// The state to pass to the function.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="func"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="SynchronizationLockException">
    /// Thrown if there is an inconsistency in the synchronization state.
    /// </exception>
    /// <remarks>
    /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
    /// or a nested (reentrant) lock depending on the calling execution flow. For reentrant locks,
    /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
    /// </remarks>
    public async ValueTask AcquireAsync<TState>(Func<TState, ValueTask> func, TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        CheckDisposed();
        bool isReentrant = LocalId == CurrentId;
        SemaphoreSlim? previousSyncRoot = null;
        if (isReentrant)//Reentrant lock in the flow
        {
            ValidateReentrantState();
            previousSyncRoot = SyncRoot;
            if (previousSyncRoot == null)
            {
                ThrowSynchronizationLockException();
            }
            await previousSyncRoot!.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires a nested lock in the flow
        }
        else
        {
            await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires an exclusive lock in the flow
            ValidateExclusiveState();
            _currentId = LocalId;//The lock is obtained
        }
        Interlocked.Increment(ref _currentCount);
        using SemaphoreSlim localSyncRoot = new(1, 1);
        SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock
        try
        {
            await func(state).ConfigureAwait(false);
        }
        finally
        {
            PerformValidationAfterExecution(isReentrant, localSyncRoot);

            // Clear this async flow's synchronization root. The caller's AsyncLocal value is preserved by its ExecutionContext.
            SyncRoot = null;

            // Do not release the underlying semaphore if ownership validation fails.
            // The lock state is no longer considered reliable.
            CheckSynchronizationLock();

            if (Interlocked.Decrement(ref _currentCount) == 0)
            {
                ValidateExclusiveStateFinally(isReentrant, previousSyncRoot);
                _currentId = 0;//The lock is released
                _syncLock.Release();//Releases an exclusive lock
            }
            else
            {
                ValidateReentrantStateFinally(isReentrant, previousSyncRoot);
                previousSyncRoot!.Release();//Releases a nested lock
            }
        }
    }

    /// <summary>
    /// Asynchronously acquires the lock for the current execution flow, executes the provided asynchronous function, and then releases the lock.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the result produced by the function.
    /// </typeparam>
    /// <param name="func">
    /// The <see cref="ValueTask{T}"/> to be executed while holding the lock. Must not be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{T}"/> representing the result of the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="func"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="SynchronizationLockException">
    /// Thrown if there is an inconsistency in the synchronization state.
    /// </exception>
    /// <remarks>
    /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
    /// or a nested (reentrant) lock depending on the calling execution flow. For reentrant locks,
    /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
    /// </remarks>
    public async ValueTask<T> AcquireAsync<T>(Func<ValueTask<T>> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        CheckDisposed();

        bool isReentrant = LocalId == CurrentId;
        SemaphoreSlim? previousSyncRoot = null;
        if (isReentrant)//Reentrant lock in the flow
        {
            ValidateReentrantState();

            previousSyncRoot = SyncRoot;
            if (previousSyncRoot == null)
            {
                ThrowSynchronizationLockException();
            }
            await previousSyncRoot!.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires a nested lock in the flow
        }
        else
        {
            await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires an exclusive lock in the flow

            ValidateExclusiveState();

            _currentId = LocalId;//The lock is obtained
        }

        Interlocked.Increment(ref _currentCount);

        using SemaphoreSlim localSyncRoot = new(1, 1);
        SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock

        try
        {
            return await func().ConfigureAwait(false);
        }
        finally
        {
            PerformValidationAfterExecution(isReentrant, localSyncRoot);

            // Clear this async flow's synchronization root. The caller's AsyncLocal value is preserved by its ExecutionContext.
            SyncRoot = null;

            // Do not release the underlying semaphore if ownership validation fails.
            // The lock state is no longer considered reliable.
            CheckSynchronizationLock();

            if (Interlocked.Decrement(ref _currentCount) == 0)
            {
                ValidateExclusiveStateFinally(isReentrant, previousSyncRoot);
                _currentId = 0;//The lock is released
                _syncLock.Release();//Releases an exclusive lock
            }
            else
            {
                ValidateReentrantStateFinally(isReentrant, previousSyncRoot);
                previousSyncRoot!.Release();//Releases a nested lock
            }
        }
    }

    /// <summary>
    /// Asynchronously acquires the lock for the current execution flow, executes the provided asynchronous function, and then releases the lock.
    /// </summary>
    /// <typeparam name="TState">
    /// The type of the state passed to the function.
    /// </typeparam>
    /// <typeparam name="TResult">
    /// The type of the result produced by the function.
    /// </typeparam>
    /// <param name="func">
    /// The asynchronous function to be executed while holding the lock. Must not be null.
    /// </param>
    /// <param name="state">
    /// The state to pass to the function.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the result of the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="func"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the object has been disposed.
    /// </exception>
    /// <exception cref="SynchronizationLockException">
    /// Thrown if there is an inconsistency in the synchronization state.
    /// </exception>
    /// <remarks>
    /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
    /// or a nested (reentrant) lock depending on the calling execution flow. For reentrant locks,
    /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
    /// </remarks>
    public async ValueTask<TResult> AcquireAsync<TState, TResult>(Func<TState, ValueTask<TResult>> func, TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        CheckDisposed();
        bool isReentrant = LocalId == CurrentId;
        SemaphoreSlim? previousSyncRoot = null;
        if (isReentrant)//Reentrant lock in the flow
        {
            ValidateReentrantState();
            previousSyncRoot = SyncRoot;
            if (previousSyncRoot == null)
            {
                ThrowSynchronizationLockException();
            }
            await previousSyncRoot!.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires a nested lock in the flow
        }
        else
        {
            await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires an exclusive lock in the flow
            ValidateExclusiveState();
            _currentId = LocalId;//The lock is obtained
        }
        Interlocked.Increment(ref _currentCount);
        using SemaphoreSlim localSyncRoot = new(1, 1);
        SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock
        try
        {
            return await func(state).ConfigureAwait(false);
        }
        finally
        {
            PerformValidationAfterExecution(isReentrant, localSyncRoot);

            // Clear this async flow's synchronization root. The caller's AsyncLocal value is preserved by its ExecutionContext.
            SyncRoot = null;

            // Do not release the underlying semaphore if ownership validation fails.
            // The lock state is no longer considered reliable.
            CheckSynchronizationLock();

            if (Interlocked.Decrement(ref _currentCount) == 0)
            {
                ValidateExclusiveStateFinally(isReentrant, previousSyncRoot);
                _currentId = 0;//The lock is released
                _syncLock.Release();//Releases an exclusive lock
            }
            else
            {
                ValidateReentrantStateFinally(isReentrant, previousSyncRoot);
                previousSyncRoot!.Release();//Releases a nested lock
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckSynchronizationLock()
    {
        if (LocalId != CurrentId)
        {
            ThrowSynchronizationLockException();
        }
    }

    /// <summary>
    /// Gets the next non-zero identifier for an execution flow.
    /// </summary>
    internal static int NewId()
    {
        int newId;

        // We need to repeat if Interlocked.Increment wraps around and returns 0.
        // Otherwise next time this Id is queried it will get a new value
        do
        {
            newId = Interlocked.Increment(ref s_idCounter);
        }
        while (newId == 0);
        return newId;
    }

    protected override void DisposeCore()
    {
        _syncLock.Dispose();
        base.DisposeCore();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowSynchronizationLockException()
    {
        throw new SynchronizationLockException();
    }

    #endregion
}