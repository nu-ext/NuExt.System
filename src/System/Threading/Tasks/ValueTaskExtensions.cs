using System.Collections.Generic;

namespace System.Threading.Tasks;

//Based on https://stackoverflow.com/questions/45689327/task-whenall-for-valuetask
/// <summary>
/// A set of extension methods for working with ValueTask objects.
/// </summary>
public static class ValueTaskExtensions
{
    extension(ValueTask)
    {
#if !NET
        /// <summary>Gets a task that has already completed successfully.</summary>
        public static ValueTask CompletedTask => default;

        /// <summary>Creates a <see cref="ValueTask{TResult}"/> that's completed successfully with the specified result.</summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="result">The result to store into the completed task.</param>
        /// <returns>The successfully completed task.</returns>
        public static ValueTask<TResult> FromResult<TResult>(TResult result) => new(result);

        /// <summary>Creates a <see cref="ValueTask"/> that has completed due to cancellation with the specified cancellation token.</summary>
        /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
        /// <returns>The canceled task.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="cancellationToken"/> has not been canceled.
        /// </exception>
        public static ValueTask FromCanceled(CancellationToken cancellationToken) =>
            new(Task.FromCanceled(cancellationToken));

        /// <summary>Creates a <see cref="ValueTask{TResult}"/> that has completed due to cancellation with the specified cancellation token.</summary>
        /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
        /// <returns>The canceled task.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="cancellationToken"/> has not been canceled.
        /// </exception>
        public static ValueTask<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) =>
            new(Task.FromCanceled<TResult>(cancellationToken));

        /// <summary>Creates a <see cref="ValueTask"/> that has completed with the specified exception.</summary>
        /// <param name="exception">The exception with which to complete the task.</param>
        /// <returns>The faulted task.</returns>
        public static ValueTask FromException(Exception exception) =>
            new(Task.FromException(exception));

        /// <summary>Creates a <see cref="ValueTask{TResult}"/> that has completed with the specified exception.</summary>
        /// <param name="exception">The exception with which to complete the task.</param>
        /// <returns>The faulted task.</returns>
        public static ValueTask<TResult> FromException<TResult>(Exception exception) =>
            new(Task.FromException<TResult>(exception));
#endif

        /// <summary>
        /// Asynchronously waits for all supplied <see cref="ValueTask"/> instances to complete.
        /// </summary>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>A task that represents the completion of all supplied tasks.</returns>
        /// <remarks>
        /// All supplied tasks are observed. If one or more tasks fail or are canceled, the returned task
        /// throws an <see cref="AggregateException"/> containing all exceptions, including
        /// <see cref="OperationCanceledException"/> instances.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="AggregateException">One or more tasks failed or were canceled.</exception>
        public static ValueTask WhenAll(IEnumerable<ValueTask> tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            int? count = null;
            if (tasks is ICollection<ValueTask> taskCollection)
            {
                count = taskCollection.Count;
                if (count is > 0 && tasks is IReadOnlyList<ValueTask> taskList)
                {
                    return WhenAll(taskList);
                }
            }

            if (count is 0)
            {
                return ValueTask.CompletedTask;
            }

            // Buffer the tasks into a temporary span. Small sets of tasks are common,
            // so for <= 8 we stack allocate.
            ValueListBuilder<ValueTask> builder = count is > 8 ?
                new ValueListBuilder<ValueTask>(count.Value) :
                new ValueListBuilder<ValueTask>([default, default, default, default, default, default, default, default]);
            foreach (ValueTask task in tasks)
            {
                builder.Append(task);
            }

            return WhenAll((IReadOnlyList<ValueTask>)builder.ToArray());
        }

        /// <summary>
        /// Asynchronously waits for all supplied <see cref="ValueTask"/> instances to complete.
        /// </summary>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>A task that represents the completion of all supplied tasks.</returns>
        /// <remarks>
        /// All supplied tasks are observed. If one or more tasks fail or are canceled, the returned task
        /// throws an <see cref="AggregateException"/> containing all exceptions, including
        /// <see cref="OperationCanceledException"/> instances.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="AggregateException">One or more tasks failed or were canceled.</exception>
        public static ValueTask WhenAll(params ValueTask[] tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            return WhenAll((IReadOnlyList<ValueTask>)tasks);
        }

        /// <summary>
        /// Asynchronously waits for all supplied <see cref="ValueTask"/> instances to complete.
        /// </summary>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>A task that represents the completion of all supplied tasks.</returns>
        /// <remarks>
        /// All supplied tasks are observed. If one or more tasks fail or are canceled, the returned task
        /// throws an <see cref="AggregateException"/> containing all exceptions, including
        /// <see cref="OperationCanceledException"/> instances.
        /// </remarks>
        /// <exception cref="AggregateException">One or more tasks failed or were canceled.</exception>
        public static ValueTask WhenAll(params ReadOnlySpan<ValueTask> tasks)
        {
            if (tasks.IsEmpty)
            {
                return ValueTask.CompletedTask;
            }
            return WhenAll((IReadOnlyList<ValueTask>)tasks.ToArray());
        }

        /// <summary>
        /// Asynchronously waits for all supplied <see cref="ValueTask{TResult}"/> instances to complete
        /// and returns their results in the same order as the supplied tasks.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the tasks.</typeparam>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>
        /// A task that represents the completion of all supplied tasks and contains their results.
        /// </returns>
        /// <remarks>
        /// All supplied tasks are observed. If one or more tasks fail or are canceled, the returned task
        /// throws an <see cref="AggregateException"/> containing all exceptions, including
        /// <see cref="OperationCanceledException"/> instances.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="AggregateException">One or more tasks failed or were canceled.</exception>
        public static ValueTask<TResult[]> WhenAll<TResult>(IEnumerable<ValueTask<TResult>> tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            int? count = null;
            if (tasks is ICollection<ValueTask<TResult>> taskCollection)
            {
                count = taskCollection.Count;
                if (count is > 0 && tasks is IReadOnlyList<ValueTask<TResult>> taskList)
                {
                    return WhenAll(taskList);
                }
            }

            if (count is 0)
            {
                return new ValueTask<TResult[]>([]);
            }

            // Buffer the tasks into a temporary span. Small sets of tasks are common,
            // so for <= 8 we stack allocate.
            ValueListBuilder<ValueTask<TResult>> builder = count is > 8 ?
                new ValueListBuilder<ValueTask<TResult>>(count.Value) :
                new ValueListBuilder<ValueTask<TResult>>([default, default, default, default, default, default, default, default]);
            foreach (ValueTask<TResult> task in tasks)
            {
                builder.Append(task);
            }

            return WhenAll((IReadOnlyList<ValueTask<TResult>>)builder.ToArray());
        }

        /// <summary>
        /// Asynchronously waits for all supplied <see cref="ValueTask{TResult}"/> instances to complete
        /// and returns their results in the same order as the supplied tasks.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the tasks.</typeparam>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>
        /// A task that represents the completion of all supplied tasks and contains their results.
        /// </returns>
        /// <remarks>
        /// All supplied tasks are observed. If one or more tasks fail or are canceled, the returned task
        /// throws an <see cref="AggregateException"/> containing all exceptions, including
        /// <see cref="OperationCanceledException"/> instances.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="AggregateException">One or more tasks failed or were canceled.</exception>
        public static ValueTask<TResult[]> WhenAll<TResult>(params ValueTask<TResult>[] tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            return WhenAll((IReadOnlyList<ValueTask<TResult>>)tasks);
        }

        /// <summary>
        /// Asynchronously waits for all supplied <see cref="ValueTask{TResult}"/> instances to complete
        /// and returns their results in the same order as the supplied tasks.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the tasks.</typeparam>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <returns>
        /// A task that represents the completion of all supplied tasks and contains their results.
        /// </returns>
        /// <remarks>
        /// All supplied tasks are observed. If one or more tasks fail or are canceled, the returned task
        /// throws an <see cref="AggregateException"/> containing all exceptions, including
        /// <see cref="OperationCanceledException"/> instances.
        /// </remarks>
        /// <exception cref="AggregateException">One or more tasks failed or were canceled.</exception>
        public static ValueTask<TResult[]> WhenAll<TResult>(params ReadOnlySpan<ValueTask<TResult>> tasks)
        {
            if (tasks.IsEmpty)
            {
                return new ValueTask<TResult[]>([]);
            }
            return WhenAll((IReadOnlyList<ValueTask<TResult>>)tasks.ToArray());
        }
    }

    /// <summary>
    /// Asynchronously waits for all supplied <see cref="ValueTask"/> instances to complete.
    /// </summary>
    /// <param name="tasks">The tasks to wait for.</param>
    /// <returns>A task that represents the completion of all supplied tasks.</returns>
    /// <remarks>
    /// All supplied tasks are observed. If one or more tasks fail or are canceled, the returned task
    /// throws an <see cref="AggregateException"/> containing all exceptions, including
    /// <see cref="OperationCanceledException"/> instances.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    /// <exception cref="AggregateException">One or more tasks failed or were canceled.</exception>
    public static async ValueTask WhenAll(this IReadOnlyList<ValueTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (tasks.Count == 0)
        {
            return;
        }

        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        for (var i = 0; i < tasks.Count; i++)
        {
            try
            {
                await tasks[i].ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>(tasks.Count - i);
                exceptions.Add(ex);
            }
        }

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }

    /// <summary>
    /// Asynchronously waits for all supplied <see cref="ValueTask{TResult}"/> instances to complete
    /// and returns their results in the same order as the supplied tasks.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the tasks.</typeparam>
    /// <param name="tasks">The tasks to wait for.</param>
    /// <returns>
    /// A task that represents the completion of all supplied tasks and contains their results.
    /// </returns>
    /// <remarks>
    /// All supplied tasks are observed. If one or more tasks fail or are canceled, the returned task
    /// throws an <see cref="AggregateException"/> containing all exceptions, including
    /// <see cref="OperationCanceledException"/> instances.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    /// <exception cref="AggregateException">One or more tasks failed or were canceled.</exception>
    public static async ValueTask<TResult[]> WhenAll<TResult>(this IReadOnlyList<ValueTask<TResult>> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (tasks.Count == 0)
        {
            return [];
        }

        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        var results = new TResult[tasks.Count];
        for (var i = 0; i < tasks.Count; i++)
        {
            try
            {
                results[i] = await tasks[i].ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>(tasks.Count - i);
                exceptions.Add(ex);
            }
        }

        return exceptions is null
            ? results
            : throw new AggregateException(exceptions);
    }
}
