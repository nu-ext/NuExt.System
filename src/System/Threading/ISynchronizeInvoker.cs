using System.Threading.Tasks;

namespace System.Threading;

/// <summary>
/// Defines a contract for an object that can synchronously and asynchronously
/// invoke delegates on its associated execution context.
/// </summary>
public interface ISynchronizeInvoker : IDispatcherObject
{
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
    public Task<object?> BeginInvoke(Delegate method, params object?[] args);

    /// <summary>
    /// Executes the specified delegate synchronously on the appropriate thread or context.
    /// </summary>
    /// <param name="method">A delegate to a method to invoke.</param>
    /// <param name="args">An array of objects to pass as arguments to the delegate, or <see langword="null"/>.</param>
    /// <returns>
    /// The return value from the delegate being invoked, or <see langword="null"/> if the delegate has no return value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is <see langword="null"/>.</exception>
    object? Invoke(Delegate method, params object?[] args);

    /// <summary>
    /// Executes the specified <see cref="Action"/> synchronously on the appropriate thread or context.
    /// </summary>
    /// <param name="callback">An <see cref="Action"/> delegate to invoke through the dispatcher object.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is null.</exception>
    void Invoke(Action callback);

    /// <summary>
    /// Executes the specified <see cref="Func{TResult}"/> synchronously on the appropriate thread or context.
    /// </summary>
    /// <typeparam name="TResult">The type of the callback return value.</typeparam>
    /// <param name="callback">A <see cref="Func{TResult}"/> delegate to invoke through the dispatcher object.</param>
    /// <returns>The return value from the delegate being invoked.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is null.</exception>
    TResult Invoke<TResult>(Func<TResult> callback);

    /// <summary>
    /// Executes the specified <see cref="Action"/> asynchronously on the appropriate thread or context.
    /// </summary>
    /// <param name="callback">An <see cref="Action"/> delegate to invoke through the dispatcher object.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is null.</exception>
    Task InvokeAsync(Action callback);

    /// <summary>
    /// Executes the specified <see cref="Func{TResult}"/> asynchronously on the appropriate thread or context.
    /// </summary>
    /// <typeparam name="TResult">The type of the callback return value.</typeparam>
    /// <param name="callback">A <see cref="Func{TResult}"/> delegate to invoke through the dispatcher object.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is null.</exception>
    Task<TResult> InvokeAsync<TResult>(Func<TResult> callback);

    /// <summary>
    /// Executes the specified Func&lt;Task&gt; asynchronously on the appropriate thread or context.
    /// </summary>
    /// <param name="callback">A Func&lt;Task&gt; delegate to invoke through the dispatcher object.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is null.</exception>
    Task InvokeAsync(Func<Task> callback);

    /// <summary>
    /// Executes the specified Func&lt;Task&lt;TResult&gt;&gt; asynchronously on the appropriate thread or context.
    /// </summary>
    /// <typeparam name="TResult">The type of the callback return value.</typeparam>
    /// <param name="callback">A Func&lt;Task&lt;TResult&gt;&gt; delegate to invoke through the dispatcher object.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is null.</exception>
    Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback);
}
