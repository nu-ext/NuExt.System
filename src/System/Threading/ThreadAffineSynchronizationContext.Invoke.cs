using System.Threading.Tasks;

namespace System.Threading;

partial class ThreadAffineSynchronizationContext
{
    #region Methods

    /// <summary>
    /// Queues the specified delegate for asynchronous execution on the associated thread or context.
    /// </summary>
    /// <param name="method">The delegate to invoke.</param>
    /// <param name="args">
    /// The arguments to pass to <paramref name="method"/>, or an empty array if the delegate has no parameters.
    /// </param>
    /// <returns>
    /// A task that completes when the delegate finishes. The task result is the delegate's return value,
    /// or <see langword="null"/> if the delegate does not return a value.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
    public Task<object?> BeginInvoke(Delegate method, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(method);

        return InnerContext.BeginInvoke(method, args);
    }

    /// <summary>
    /// Executes the specified delegate synchronously on the appropriate thread or context.
    /// </summary>
    /// <param name="method">A delegate to a method to invoke.</param>
    /// <param name="args">An array of objects to pass as arguments to the delegate, or <see langword="null"/>.</param>
    /// <returns>
    /// The return value from the delegate being invoked, or <see langword="null"/> if the delegate has no return value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// If the call is made from the thread associated with this context (as determined by <see cref="CheckAccess"/>),
    /// the delegate is executed immediately and synchronously. Otherwise, the call is dispatched to the correct thread
    /// and the current thread blocks until the operation completes.
    /// </para>
    /// </remarks>
    public object? Invoke(Delegate method, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (CheckAccess())
        {
            return method.Call(args);
        }

        return InnerContext.Invoke(method, args);
    }

    /// <summary>
    /// Executes the specified <see cref="Action"/> synchronously on the appropriate thread or context.
    /// </summary>
    /// <param name="callback">An <see cref="Action"/> delegate to invoke.</param>
    /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/exception"/>
    /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/remarks"/>
    public void Invoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback();
            return;
        }

        InnerContext.Invoke(callback);
    }

    /// <summary>
    /// Executes the specified <see cref="Func{TResult}"/> synchronously on the appropriate thread or context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the delegate.</typeparam>
    /// <param name="callback">A <see cref="Func{TResult}"/> delegate to invoke.</param>
    /// <returns>The result returned by <paramref name="callback"/>.</returns>
    /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/exception"/>
    /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/remarks"/>
    public TResult Invoke<TResult>(Func<TResult> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            return callback();
        }

        return InnerContext.Invoke(callback);
    }

    /// <summary>
    /// Executes the specified <see cref="Action"/> asynchronously on the appropriate thread or context.
    /// </summary>
    /// <param name="callback">An <see cref="Action"/> delegate to invoke.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/remarks"/>
    public Task InvokeAsync(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return InnerContext.InvokeAsync(callback);
    }

    /// <summary>
    /// Executes the specified <see cref="Func{TResult}"/> asynchronously on the appropriate thread or context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the delegate.</typeparam>
    /// <inheritdoc cref="InvokeAsync(Action)" path="/param[@name='callback']"/>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation.</returns>
    /// <inheritdoc cref="InvokeAsync(Action)" path="/exception"/>
    /// <inheritdoc cref="InvokeAsync(Action)" path="/remarks"/>
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return InnerContext.InvokeAsync(callback);
    }

    /// <summary>
    /// Executes the specified asynchronous delegate that returns a <see cref="Task"/> on the appropriate thread or context.
    /// </summary>
    /// <param name="callback">A <see cref="Func{TResult}"/> delegate that returns a <see cref="Task"/> to invoke, where <c>TResult</c> is <see cref="Task"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the execution of the provided async delegate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method is suitable for invoking asynchronous methods (async/await) in a thread-safe manner.
    /// </para>
    /// </remarks>
    public Task InvokeAsync(Func<Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return InnerContext.InvokeAsync(callback);
    }

    /// <summary>
    /// Executes the specified asynchronous delegate that returns a <see cref="Task{TResult}"/> on the appropriate thread or context.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the returned <see cref="Task{TResult}"/>.</typeparam>
    /// <param name="callback">
    /// A delegate that returns the <see cref="Task{TResult}"/> to invoke.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the execution of the provided async delegate. Its result is the result
    /// of the <see cref="Task{TResult}"/> returned by <paramref name="callback"/>.
    /// </returns>
    /// <inheritdoc cref="InvokeAsync(Func{Task})" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method is suitable for invoking asynchronous methods (async/await) that return a value in a thread-safe manner.
    /// </para>
    /// </remarks>
    public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return InnerContext.InvokeAsync(callback);
    }

    #endregion
}