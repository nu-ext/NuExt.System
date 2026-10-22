using System.ComponentModel;
using System.Threading.Tasks;

namespace System.Threading;

/// <summary>
/// Provides asynchronous waiting for a <see cref="WaitHandle"/> with support for timeouts
/// and cancellation.
/// </summary>
/// <remarks>
/// <para>
/// The lifetime of the underlying wait handle is controlled by the
/// <c>ownsWaitObject</c> constructor parameter.
/// </para>
/// <para>
/// When this instance owns the underlying wait handle, dispose this instance only after all
/// pending wait operations have completed. Disposal is not synchronized with active waits
/// and does not cancel them.
/// </para>
/// <para>
/// When this instance does not own the underlying wait handle, the caller is responsible for
/// keeping the wait handle alive until all pending wait operations have completed.
/// </para>
/// </remarks>
public sealed class AsyncWaitHandle : Disposable
{
    private readonly WaitHandle _waitObject;
    private readonly bool _ownsWaitObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncWaitHandle"/> class.
    /// </summary>
    /// <param name="waitObject">
    /// The <see cref="WaitHandle"/> to use for synchronization.
    /// </param>
    /// <param name="ownsWaitObject">
    /// <see langword="true"/> to dispose <paramref name="waitObject"/> when this instance is disposed;
    /// <see langword="false"/> to leave it open. When <see langword="false"/>, the caller is responsible
    /// for keeping <paramref name="waitObject"/> alive until all pending wait operations have completed.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="waitObject"/> is <see langword="null"/>.
    /// </exception>
    public AsyncWaitHandle(WaitHandle waitObject, bool ownsWaitObject = true)
    {
        ArgumentNullException.ThrowIfNull(waitObject);

        _waitObject = waitObject;
        _ownsWaitObject = ownsWaitObject;
    }

    #region Methods

    /// <inheritdoc/>
    protected override void DisposeCore()
    {
        if (_ownsWaitObject)
        {
            _waitObject.Dispose();
        }

        base.DisposeCore();
    }

    /// <summary>
    /// Asynchronously waits indefinitely until the wait handle is signaled or the operation is canceled.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token used to cancel the wait operation.
    /// </param>
    /// <returns>
    /// A task that completes with <see langword="true"/> when the wait handle is signaled.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// This instance or the underlying <see cref="WaitHandle"/> has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="cancellationToken"/> was canceled.
    /// </exception>
    public Task<bool> WaitOneAsync(CancellationToken cancellationToken = default)
    {
        return WaitOneAsync(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    /// <summary>
    /// Asynchronously waits until the wait handle is signaled, the specified timeout elapses,
    /// or the operation is canceled.
    /// </summary>
    /// <param name="millisecondsTimeout">
    /// The number of milliseconds to wait, <see cref="Timeout.Infinite"/> to wait indefinitely,
    /// or zero to test the state of the wait handle and return immediately.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token used to cancel the wait operation.
    /// </param>
    /// <returns>
    /// A task that completes with <see langword="true"/> if the wait handle was signaled;
    /// otherwise, <see langword="false"/> if the timeout elapsed.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="millisecondsTimeout"/> is less than <see cref="Timeout.Infinite"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// This instance or the underlying <see cref="WaitHandle"/> has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="cancellationToken"/> was canceled.
    /// </exception>
    public Task<bool> WaitOneAsync(int millisecondsTimeout, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsTimeout, Timeout.Infinite);

        return WaitOneAsync(TimeSpan.FromMilliseconds(millisecondsTimeout), cancellationToken);
    }

    /// <summary>
    /// Asynchronously waits until the wait handle is signaled, the specified timeout elapses,
    /// or the operation is canceled.
    /// </summary>
    /// <param name="timeout">
    /// The maximum time to wait, <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely,
    /// or <see cref="TimeSpan.Zero"/> to test the state of the wait handle and return immediately.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token used to cancel the wait operation.
    /// </param>
    /// <returns>
    /// A task that completes with <see langword="true"/> if the wait handle was signaled;
    /// otherwise, <see langword="false"/> if the timeout elapsed.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="timeout"/> is a negative value other than
    /// <see cref="Timeout.InfiniteTimeSpan"/>, or exceeds the maximum supported timeout.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// This instance or the underlying <see cref="WaitHandle"/> has been disposed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="cancellationToken"/> was canceled.
    /// </exception>
    public async Task<bool> WaitOneAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        CheckDisposed();

        ArgumentOutOfRangeException.ThrowIfLessThan(timeout, Timeout.InfiniteTimeSpan);
        if (timeout.TotalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (_waitObject.WaitOne(0))
        {
            return true;
        }

        if (timeout == TimeSpan.Zero)
        {
            return false;
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void WaitOrTimerCallback(object? state, bool timedOut)
        {
            tcs.TrySetResult(!timedOut);
        }

        var handle = ThreadPool.RegisterWaitForSingleObject(_waitObject, WaitOrTimerCallback, null, timeout, true);
        try
        {
            using (cancellationToken.CanBeCanceled
                       ? cancellationToken
#if NET
                       .UnsafeRegister(static state =>
                       {
                           var (tcs, cancellationToken) = ((TaskCompletionSource<bool>, CancellationToken))state!;
                           tcs.TrySetCanceled(cancellationToken);
                       }, (tcs, cancellationToken))
#else
                       .Register(static state =>
                       {
                           var (tcs, cancellationToken) = ((TaskCompletionSource<bool>, CancellationToken))state!;
                           tcs.TrySetCanceled(cancellationToken);
                       }, (tcs, cancellationToken), useSynchronizationContext: false)
#endif
                       : null as IDisposable)
            {
                return await tcs.Task.ConfigureAwait(false);
            }
        }
        finally
        {
            handle.Unregister(null);
        }
    }

    #endregion
}