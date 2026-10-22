using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading;

public static class SynchronizationContextExtensions
{
    private static readonly HashSet<string> s_knownThreadAffineContexts = new (StringComparer.Ordinal)
    {
        "System.Windows.Threading.DispatcherSynchronizationContext",
        "System.Windows.Forms.WindowsFormsSynchronizationContext",
        "Avalonia.Threading.AvaloniaSynchronizationContext",
        "System.Threading.ThreadAffineSynchronizationContext"
    };

    extension (SynchronizationContext synchronizationContext)
    {
        /// <summary>
        /// Determines if the synchronization context is recognized as a known thread-affine type.
        /// </summary>
        /// <value>
        /// <see langword="true"/> for contexts like WPF's DispatcherSynchronizationContext,
        /// Windows Forms' WindowsFormsSynchronizationContext, or Avalonia's AvaloniaSynchronizationContext;
        /// otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This check recognizes implementations of <see cref="IThreadAffineSynchronizationContext"/>
        /// and a predefined set of synchronization context type names. A value of
        /// <see langword="true"/> indicates that the context type is known to be thread-affine;
        /// it does not guarantee that an access-checking delegate can be created for that instance.
        /// A value of <see langword="false"/> does not prove that the context is not thread-affine.
        /// </remarks>
        public bool IsThreadAffineKnown =>
            synchronizationContext is IThreadAffineSynchronizationContext ||
            s_knownThreadAffineContexts.Contains(synchronizationContext.GetType().FullName!);

        /// <summary>
        /// Executes the specified delegate synchronously using the <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="d">The delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        /// <returns>The return value from the delegate being invoked, or <see langword="null"/> if the delegate has no return value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="d"/> parameter is <see langword="null"/>.</exception>
        public object? Send(Func<object?, object?> d, object? state)
        {
            ArgumentNullException.ThrowIfNull(d);

            if (ReferenceEquals(SynchronizationContext.Current, synchronizationContext)) 
            { 
                return d(state);
            }

            object? result = null;
            synchronizationContext.Send(x => result = d(x), state);
            return result;
        }

        /// <summary>
        /// Executes the specified delegate synchronously using the <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="d">The delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        /// <returns>The return value from the delegate being invoked.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="d"/> parameter is null.</exception>
        public TResult Send<TResult>(Func<object?, TResult> d, object? state)
        {
            ArgumentNullException.ThrowIfNull(d);

            if (ReferenceEquals(SynchronizationContext.Current, synchronizationContext))
            {
                return d(state);
            }

            TResult result = default!;
            synchronizationContext.Send(x => result = d(x), state);
            return result;
        }

        /// <summary>
        /// Queues the specified delegate for asynchronous execution on the synchronization context.
        /// </summary>
        /// <param name="method">The delegate to invoke.</param>
        /// <param name="args">
        /// The arguments to pass to <paramref name="method"/>, or an empty array if the delegate has no parameters.
        /// </param>
        /// <returns>
        /// A task that completes when the delegate finishes. The task result is the delegate's return value,
        /// or <see langword="null"/> if the delegate does not return a value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="method"/> is <see langword="null"/>.
        /// </exception>
        public Task<object?> BeginInvoke(Delegate method, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(method);

            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            synchronizationContext.Post(_ =>
            {
                try
                {
                    object? result = method.Call(args);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;
        }

        /// <summary>
        /// Executes the specified delegate synchronously using the <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="method">A delegate to a method that takes parameters of the same number and type that are contained in the args parameter.</param>
        /// <param name="args">An array of objects to pass as arguments to the given method. This can be <see langword="null"/> if no arguments are needed.</param>
        /// <returns>The return value from the delegate being invoked, or <see langword="null"/> if the delegate has no return value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="method"/> parameter is <see langword="null"/>.</exception>
        public object? Invoke(Delegate method, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(method);

            if (ReferenceEquals(SynchronizationContext.Current, synchronizationContext))
            {
                return method.Call(args);
            }

            object? result = null;
            synchronizationContext.Send(_ => result = method.Call(args), null);
            return result;
        }

        /// <summary>
        /// Executes the specified delegate synchronously using the <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="callback">The <see cref="Action"/> delegate to call.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        public void Invoke(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (ReferenceEquals(SynchronizationContext.Current, synchronizationContext))
            {
                callback();
                return;
            }

            synchronizationContext.Send(_ => callback(), null);
        }

        /// <summary>
        /// Executes the specified delegate synchronously using the <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="callback">The <see cref="Func{TResult}"/> delegate to call.</param>
        /// <returns>The return value from the delegate being invoked.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (ReferenceEquals(SynchronizationContext.Current, synchronizationContext))
            {
                return callback();
            }

            TResult result = default!;
            synchronizationContext.Send(_ => result = callback(), null);
            return result;
        }

        /// <summary>
        /// Posts an asynchronous message to the synchronization context and returns a task that represents the operation.
        /// </summary>
        /// <param name="d">The <see cref="SendOrPostCallback"/> delegate to call.</param>
        /// <param name="state">An object passed to the delegate.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="d"/> parameter is <see langword="null"/>.</exception>
        /// <remarks>
        /// The method allows the delegate to be executed on a specific thread or context. The returned task completes 
        /// when the delegate has finished executing, either successfully or with an exception.
        /// </remarks>
        public Task InvokeAsync(SendOrPostCallback d, object? state)
        {
            ArgumentNullException.ThrowIfNull(d);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            synchronizationContext.Post(x => 
            {
                try
                {
                    d(x);
                    tcs.SetResult(true);
                }
                catch (Exception ex) 
                { 
                    tcs.SetException(ex); 
                }
            }, state);

            return tcs.Task;
        }

        /// <summary>
        /// Posts an asynchronous message to the synchronization context and returns a task that represents the operation.
        /// </summary>
        /// <param name="callback">The <see cref="Action"/> delegate to call.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        /// <remarks>
        /// The method allows the delegate to be executed on a specific thread or context. The returned task completes 
        /// when the delegate has finished executing, either successfully or with an exception.
        /// </remarks>
        public Task InvokeAsync(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            synchronizationContext.Post(_ =>
            {
                try
                {
                    callback();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;
        }

        /// <summary>
        /// Posts an asynchronous message to the synchronization context and returns a task that represents the operation.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="callback">The <see cref="Func{TResult}"/> delegate to call.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        /// <remarks>
        /// The method allows the delegate to be executed on a specific thread or context. The returned task completes 
        /// when the delegate has finished executing, either successfully or with an exception.
        /// </remarks>
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            synchronizationContext.Post(_ =>
            {
                try
                {
                    TResult result = callback();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;
        }

        /// <summary>
        /// Posts an asynchronous message to the synchronization context and returns a task that represents the operation.
        /// </summary>
        /// <param name="callback">The Func&lt;Task&gt; delegate to call.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        /// <remarks>
        /// The method allows the callback to be awaited and executed on a specific thread or context.
        /// The returned task completes when the delegate has finished executing, either successfully or with an exception.
        /// </remarks>
        public Task InvokeAsync(Func<Task> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            synchronizationContext.Post(async void (_) =>
            {
                try
                {
                    await callback().ConfigureAwait(false);
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;
        }

        /// <summary>
        /// Posts an asynchronous message to the synchronization context and returns a task that represents the operation.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="callback">The Func&lt;Task&lt;TResult&gt;&gt; delegate to call.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        /// <remarks>
        /// The method allows the callback to be awaited and executed on a specific thread or context.
        /// The returned task completes when the delegate has finished executing, either successfully or with an exception.
        /// </remarks>
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            synchronizationContext.Post(async void (_) =>
            {
                try
                {
                    TResult result = await callback().ConfigureAwait(false);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;
        }

    }
}
