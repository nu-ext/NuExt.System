using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.ComponentModel;

/// <summary>
/// Provides a base class for objects that can be disposed asynchronously exactly once,
/// with property change notifications for their disposal state.
/// </summary>
/// <remarks>
/// <para>
/// This class implements the <see cref="IAsyncDisposableNotifiable"/> interface, providing the
/// <see cref="Disposing"/> event and exposing its disposal state (<see cref="IsDisposing"/>, <see cref="IsDisposed"/>).
/// </para>
/// <para>
/// The <see cref="DisposeAsync"/> method is thread-safe and designed to be called exactly once. 
/// Override <see cref="DisposeAsyncCore"/> to release managed resources asynchronously. 
/// The <see cref="Dispose(bool)"/> method (called with <see langword="false"/>)
/// is reserved for releasing unmanaged resources.
/// </para>
/// </remarks>
[DebuggerStepThrough]
public abstract class AsyncDisposable : PropertyChangeNotifier, IAsyncDisposableNotifiable
{
#if NET9_0_OR_GREATER
    private enum States
    {
        NotDisposed,// default value of _state
        Disposing,
        Disposed
    }

    private volatile States _state;
#else
    private static class States
    {
        public const int NotDisposed = 0;// default value of _state
        public const int Disposing = 1;
        public const int Disposed = 2;
    }

    private volatile int _state;
#endif
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDisposable"/> class 
    /// without a synchronization context.
    /// </summary>
    protected AsyncDisposable() { }


    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDisposable"/> class 
    /// with the specified synchronization context.
    /// </summary>
    /// <param name="synchronizationContext">
    /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
    /// If null, no synchronization context will be used.
    /// </param>
    protected AsyncDisposable(SynchronizationContext? synchronizationContext) : base(synchronizationContext) { }

    /// <summary>
    /// Finalizes an instance of the <see cref="AsyncDisposable"/> class.
    /// This method is invoked by the garbage collector if explicit disposal did not occur.
    /// It releases unmanaged resources by calling <see cref="Dispose(bool)"/> with disposing set to <see langword="false"/>.
    /// </summary>
    ~AsyncDisposable()
    {
        try
        {
            Dispose(false);
            if (!Debugger.IsAttached) return;
            IDebuggable debugInfo = this;
            string message = $"{GetType().FullName} ({RuntimeHelpers.GetHashCode(this):X8}) was finalized without proper disposal: ({debugInfo.DebugInfo})";
            Trace.WriteLine(message);
            Debug.Fail(message);
        }
        catch (Exception ex)
        {
            IDebuggable debugInfo = this;
            string message = $"{GetType().FullName} ({RuntimeHelpers.GetHashCode(this):X8}) was finalized without proper disposal and disposed with exception: ({debugInfo.DebugInfo}){Environment.NewLine}{ex.Message}";
            Trace.WriteLine(message);
            Debug.Fail(message);
        }
    }

    #region Properties

    /// <summary>
    /// Gets or initializes a value indicating whether to capture the current synchronization context 
    /// and use it to execute asynchronous continuations during disposal.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to execute continuations on the captured synchronization context;
    /// <see langword="false"/> to execute continuations on a thread pool thread. 
    /// The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Controls the flow of the synchronization context during asynchronous operations.
    /// </para>
    /// <para>
    /// Set to <see langword="true"/> when the instance must interact with thread-affine objects
    /// (e.g., UI controls in WPF/WinForms). Keep as <see langword="false"/> for library code.
    /// </para>
    /// <para>
    /// If <see cref="ContinueOnCapturedContext"/> is <see langword="true"/>, both the <see cref="Disposing"/> handlers
    /// and <see cref="DisposeAsyncCore"/> continuations run on the captured SynchronizationContext.
    /// </para>
    /// </remarks>
    public bool ContinueOnCapturedContext { get; init; }

    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    /// <remarks>
    /// This property is thread-safe for reading.
    /// </remarks>
    [Browsable(false)]
    public bool IsDisposed => _state == States.Disposed;

    /// <summary>
    /// Gets a value indicating whether the object is currently in the process of being disposed.
    /// </summary>
    /// <remarks>
    /// This property is thread-safe for reading.
    /// </remarks>
    [Browsable(false)]
    public bool IsDisposing => _state == States.Disposing;

    /// <summary>
    /// Gets a value indicating whether the object is currently disposing or has been disposed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the object is in the <see cref="IsDisposing"/> or 
    /// <see cref="IsDisposed"/> state; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property is thread-safe for reading.
    /// </remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [Browsable(false)]
    protected bool IsDisposingOrDisposed => _state != States.NotDisposed;

    /// <summary>
    /// Gets or initializes whether to throw an <see cref="ObjectDisposedException"/>
    /// when <see cref="DisposeAsync"/> is called on an already disposed instance.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to throw an exception on redundant dispose attempts;
    /// <see langword="false"/> to make redundant calls a no-op.
    /// Default is <see langword="false"/> (redundant calls are ignored).
    /// </value>
    /// <remarks>
    /// <para>
    /// Redundant calls occur when <see cref="DisposeAsync"/> is invoked after 
    /// the object has already transitioned to the <see cref="IsDisposed"/> state.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public bool ThrowOnRedundantDispose { get; init; }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the object starts the disposing process asynchronously.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Subscribers are responsible for unsubscribing from this event to prevent memory leaks.
    /// The event is not automatically cleared during disposal.
    /// </para>
    /// <para>
    /// Event handlers should complete quickly. Exceptions thrown by handlers will propagate
    /// to the caller of <see cref="DisposeAsync"/> and may interrupt the disposal process.
    /// </para>
    /// </remarks>
    public event AsyncEventHandler? Disposing;

    #endregion

    #region Methods

    /// <summary>
    /// Checks if the object has been disposed and throws an <see cref="ObjectDisposedException"/> if it has.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the object is already disposed.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void CheckDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    /// <summary>
    /// Checks if the object has been disposed or is in the process of disposing.
    /// Throws an <see cref="ObjectDisposedException"/> if either condition is true.
    /// </summary>
    /// <remarks>
    /// This method provides a stronger guarantee than <see cref="CheckDisposed"/> by also
    /// validating that the object is not currently in the disposal process, which helps
    /// prevent race conditions during asynchronous disposal.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the object is either already disposed or currently being disposed.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void CheckDisposingOrDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposingOrDisposed, this);
    }

    /// <summary>
    /// Asynchronously releases all resources used by this instance. This method is idempotent and thread-safe.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed and
    /// <see cref="ThrowOnRedundantDispose"/> returns <see langword="true"/>.</exception>
    /// <remarks>
    /// Invokes the <see cref="Disposing"/> event, calls <see cref="DisposeAsyncCore"/> for managed resources
    /// and calls <see cref="Dispose(bool)"/> with <see langword="false"/> for unmanaged resources.
    /// The disposal operation is performed only once. Subsequent calls are thread-safe and have no effect.
    /// The object transitions to the disposed state even if an exception occurs during cleanup.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _state, States.Disposing, States.NotDisposed) != States.NotDisposed)
        {
            // Already disposing or disposed

            if (IsDisposed)
            {
                string message = $"{GetType().FullName} ({RuntimeHelpers.GetHashCode(this):X8}) is already disposed.";
                Trace.WriteLine(message);
                Debug.Fail(message);
            }
            if (IsDisposing)
            {
                string message = $"{GetType().FullName} ({RuntimeHelpers.GetHashCode(this):X8}) is already disposing.";
                Trace.WriteLine(message);
                Debug.Fail(message);
            }

            if (ThrowOnRedundantDispose)
            {
                ObjectDisposedException.ThrowIf(IsDisposed, this);
            }
            return;
        }

        OnPropertyChanged(EventArgsCache.IsDisposingPropertyChanged);
        try
        {
            if (Disposing is { } disposing)
            {
                await disposing.
                    InvokeAsync(this, EventArgs.Empty, continueOnCapturedContext: ContinueOnCapturedContext, continueOnException: false, CancellationToken.None)
                    .ConfigureAwait(continueOnCapturedContext: ContinueOnCapturedContext);
            }
            //https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
            await DisposeAsyncCore().ConfigureAwait(continueOnCapturedContext: ContinueOnCapturedContext);
            Dispose(false);

            if (Disposing != null)
            {
                var message = $"{GetType().FullName} ({RuntimeHelpers.GetHashCode(this):X8}): {nameof(Disposing)} is not null";
                Trace.WriteLine(message);
                Debug.Fail(message);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"{GetType().FullName} ({RuntimeHelpers.GetHashCode(this):X8}):{Environment.NewLine}{ex.Message}";
            Trace.WriteLine(errorMessage);
            Debug.Fail(errorMessage);
            throw;
        }
        finally
        {
            Disposing = null;
            _state = States.Disposed;
            GC.SuppressFinalize(this);
            RaisePropertiesChangedAndClearHandlers(EventArgsCache.IsDisposingPropertyChanged, EventArgsCache.IsDisposedPropertyChanged);
        }
    }

    /// <summary>
    /// Override to asynchronously release managed resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous cleanup operation.</returns>
    /// <remarks>
    /// This method is called by <see cref="DisposeAsync"/> before releasing unmanaged resources.
    /// The base implementation returns a completed task. Derived classes should override this method to release managed resources
    /// and should call the base implementation to ensure proper cleanup in the inheritance chain.
    /// </remarks>
    protected virtual ValueTask DisposeAsyncCore()
    {
        return default;
    }

    /// <summary>
    /// Releases the unmanaged resources used by this instance.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// In this asynchronous disposal pattern, the parameter is always <see langword="false"/>.
    /// </param>
    /// <remarks>
    /// This method is called by <see cref="DisposeAsync"/> and the finalizer (if required).
    /// This method is always called with <paramref name="disposing"/> set to <see langword="false"/> in this
    /// asynchronous disposal pattern. It must be idempotent and must not throw exceptions.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
    }

    #endregion
}
