using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Threading;

/// <summary>
/// Provides support for asynchronous lazy initialization. This type guarantees thread-safe,
/// one-time execution of the initialization logic and caches the resulting task.
/// </summary>
/// <typeparam name="T">The type of the lazily initialized value.</typeparam>
/// <remarks>
/// <para>
/// Unlike <see cref="Lazy{T}"/>, which provides synchronous lazy initialization,
/// <see cref="AsyncLazy{T}"/> represents an asynchronous initialization pattern in which
/// the initialized value is exposed through a cached <see cref="Task{T}"/>.
/// </para>
/// <para>
/// The initialization factory is executed only once, upon the first access to
/// the <see cref="Task"/> property or when the instance is awaited. All subsequent
/// calls receive the same cached task, regardless of its completion state.
/// </para>
/// </remarks>
public sealed class AsyncLazy<T>
{
    private readonly Lazy<Task<T>> _lazyTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class.
    /// </summary>
    /// <param name="factory">
    /// The asynchronous factory delegate that produces the value when invoked.
    /// </param>
    /// <param name="runSynchronously">
    /// <para>
    /// <see langword="true"/> to invoke <paramref name="factory"/> directly from the underlying
    /// lazy initializer. The synchronous portion of the factory executes on the thread that first
    /// accesses the value and while the lazy initializer is executing its value factory.
    /// </para>
    /// <para>
    /// <see langword="false"/> to invoke <paramref name="factory"/> through
    /// <see cref="Task.Run{T}(Func{Task{T}})"/> on a thread pool thread. This prevents the
    /// synchronous portion of the factory from running inside the lazy initializer and from
    /// capturing the caller's current synchronization context.
    /// </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public AsyncLazy(Func<Task<T>> factory, bool runSynchronously = false)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _lazyTask = new Lazy<Task<T>>(runSynchronously ? factory : () => System.Threading.Tasks.Task.Run(factory));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class for CPU-bound,
    /// synchronous initialization work.
    /// </summary>
    /// <param name="factory">
    /// The synchronous factory delegate that produces the value when invoked.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This constructor is intended for CPU-intensive, synchronous work. The provided
    /// <paramref name="factory"/> delegate is executed on a thread pool thread through
    /// <see cref="Task.Run{T}(Func{T})"/>, preventing the factory from executing on the thread
    /// that first accesses the lazy value.
    /// </para>
    /// <para>
    /// For I/O-bound or naturally asynchronous work, use the constructor accepting a
    /// <see cref="Func{TResult}"/> that returns a <see cref="Task{TResult}"/>.
    /// </para>
    /// </remarks>
    public AsyncLazy(Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _lazyTask = new Lazy<Task<T>>(() => System.Threading.Tasks.Task.Run(factory));
    }

    /// <summary>
    /// Gets a value indicating whether the initialization task has been created.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the underlying task factory has been invoked and a
    /// <see cref="Task{T}"/> has been created; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property returning <see langword="true"/> does not guarantee the task has completed.
    /// It only indicates that the initialization process has started.
    /// </remarks>
    public bool IsTaskCreated => _lazyTask.IsValueCreated;

    /// <summary>
    /// Gets a value indicating whether the initialization task has completed successfully.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the task has been created and has run to completion
    /// (<see cref="Task.Status"/> equals <see cref="TaskStatus.RanToCompletion"/>);
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool IsCompletedSuccessfully => IsTaskCreated && Task.Status == TaskStatus.RanToCompletion;

    /// <summary>
    /// Gets a value indicating whether the initialization task has completed due to an unhandled exception.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the task has been created and has faulted
    /// (<see cref="Task.IsFaulted"/> is <see langword="true"/>); otherwise, <see langword="false"/>.
    /// </value>
    public bool IsFaulted => IsTaskCreated && Task.IsFaulted;

    /// <summary>
    /// Infrastructure. Returns an awaiter used to await this asynchronous lazy initialization.
    /// </summary>
    /// <returns>An awaiter instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();

    /// <summary>
    /// Configures an awaiter used to await this asynchronous lazy initialization.
    /// </summary>
    /// <param name="continueOnCapturedContext">
    /// <see langword="true"/> to attempt to marshal the continuation back to the original
    /// context captured; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>An object used to await this task.</returns>
    public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext) => Task.ConfigureAwait(continueOnCapturedContext);

    /// <summary>
    /// Gets the <see cref="Task{T}"/> that represents the asynchronous initialization.
    /// </summary>
    /// <value>
    /// A <see cref="Task{T}"/> that, upon completion, provides the lazily initialized value.
    /// Accessing this property for the first time triggers the execution of the factory delegate.
    /// Subsequent accesses return the same cached task instance.
    /// </value>
    /// <remarks>
    /// The task's result is cached. If the task completes successfully, subsequent awaits
    /// will get the result immediately. If the task faults, the same faulted task will be
    /// returned on subsequent accesses, and the exception will be re-thrown on every await.
    /// </remarks>
    public Task<T> Task => _lazyTask.Value;
}
