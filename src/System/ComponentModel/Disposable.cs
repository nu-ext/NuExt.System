using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.ComponentModel;

/// <summary>
/// Provides a base class for objects that can be disposed synchronously exactly once,
/// with property change notifications for their disposal state.
/// </summary>
/// <remarks>
/// <para>
/// This class implements the <see cref="IDisposableNotifiable"/> interface,
/// exposing its disposal state (<see cref="IsDisposing"/>, <see cref="IsDisposed"/>).
/// </para>
/// <para>
/// The <see cref="Dispose()"/> method is thread-safe and designed to be called exactly once.
/// Override <see cref="DisposeCore"/> to release managed resources.
/// </para>
/// </remarks>
[DebuggerStepThrough]
public abstract class Disposable : PropertyChangeNotifier, IDisposableNotifiable
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
    /// Initializes a new instance of the <see cref="Disposable"/> class 
    /// without a synchronization context.
    /// </summary>
    protected Disposable() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Disposable"/> class 
    /// with the specified synchronization context.
    /// </summary>
    /// <param name="synchronizationContext">
    /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
    /// If null, no synchronization context will be used.
    /// </param>
    protected Disposable(SynchronizationContext? synchronizationContext) : base(synchronizationContext) { }

    /// <summary>
    /// Finalizes an instance of the <see cref="Disposable"/> class.
    /// This method is invoked by the garbage collector if explicit disposal did not occur.
    /// It releases unmanaged resources by calling <see cref="Dispose(bool)"/> with disposing set to <see langword="false"/>.
    /// </summary>
    ~Disposable()
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
    /// when <see cref="Dispose()"/> is called on an already disposed instance.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to throw an exception on redundant dispose attempts;
    /// <see langword="false"/> to make redundant calls a no-op.
    /// Default is <see langword="false"/> (redundant calls are ignored).
    /// </value>
    /// <remarks>
    /// <para>
    /// Redundant calls occur when <see cref="Dispose()"/> is invoked after 
    /// the object has already transitioned to the <see cref="IsDisposed"/> state.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public bool ThrowOnRedundantDispose { get; init; }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the object starts the disposing process.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event is raised <b>synchronously</b> during the call to <see cref="Dispose()"/>.
    /// Event handlers must execute quickly and must not perform blocking operations.
    /// Long-running handlers will block the disposal thread indefinitely and may cause deadlocks.
    /// </para>
    /// <para>
    /// Subscribers are responsible for unsubscribing to prevent memory leaks.
    /// The event is not automatically cleared.
    /// </para>
    /// <para>
    /// Exceptions thrown by handlers propagate to the caller of <see cref="Dispose()"/>.
    /// </para>
    /// <para>
    /// For asynchronous cleanup, use <see cref="AsyncDisposable"/> and its 
    /// <see cref="AsyncDisposable.Disposing"/> event instead.
    /// </para>
    /// </remarks>
    public event EventHandler? Disposing;

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
    /// Releases all resources used by this instance. This method is thread‑safe and designed to be called exactly once.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed and
    /// <see cref="ThrowOnRedundantDispose"/> returns <see langword="true"/>.</exception>
    /// <remarks>
    /// Invokes the <see cref="Disposing"/> event and calls <see cref="Dispose(bool)"/> with disposing set to <see langword="true"/>,
    /// which in turn invokes <see cref="DisposeCore"/> for managed resources.
    /// The disposal operation is performed only once. Subsequent calls are thread-safe and have no effect.
    /// The object transitions to the disposed state even if an exception occurs during cleanup.
    /// </remarks>
    public void Dispose()
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
            Disposing?.Invoke(this, EventArgs.Empty);
            //https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
            Dispose(true);

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
    /// Override to release managed resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called from <see cref="Dispose(bool)"/> when the disposing parameter is <see langword="true"/>.
    /// It is not called during finalization.
    /// </para>
    /// <para>
    /// The base implementation does nothing. Derived classes should override this method to release managed resources
    /// and should call the base implementation to ensure proper cleanup in the inheritance chain.
    /// </para>
    /// </remarks>
    protected virtual void DisposeCore()
    {
    }

    /// <summary>
    /// Releases the unmanaged resources used by this instance and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources (when called from <see cref="Dispose()"/>);
    /// <see langword="false"/> to release only unmanaged resources (when called from a finalizer).
    /// </param>
    /// <remarks>
    /// <para>
    /// This method is called by <see cref="Dispose()"/> (with <paramref name="disposing"/> set to <see langword="true"/>) and by the finalizer (with <paramref name="disposing"/> set to <see langword="false"/>).
    /// </para>
    /// <para>
    /// When overriding this method in a derived class, release unmanaged resources regardless of the value of <paramref name="disposing"/>.
    /// Release managed resources only when <paramref name="disposing"/> is <see langword="true"/>, preferably by overriding <see cref="DisposeCore"/>.
    /// </para>
    /// <para>
    /// This method must be idempotent and safe to call multiple times. It must not throw exceptions when <paramref name="disposing"/> is <see langword="false"/>.
    /// </para>
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeCore();
        }
    }

    /// <summary>
    /// Disposes of the specified disposable object and sets the reference to null.
    /// </summary>
    /// <typeparam name="T">The type of the disposable object.</typeparam>
    /// <param name="disposable">The disposable object to dispose of.</param>
    /// <returns>The original disposable object, or null if it was already null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable? DisposeAndNull<T>(ref T? disposable) where T : class, IDisposable
    {
        var original = disposable;
        disposable = null;
        original?.Dispose();
        return original;
    }

    /// <summary>
    /// Disposes of the specified disposable object and sets the reference to null.
    /// Returns a value indicating whether the object was disposed.
    /// </summary>
    /// <typeparam name="T">The type of the disposable object.</typeparam>
    /// <param name="disposable">The disposable object to dispose of.</param>
    /// <returns><see langword="true"/> if the disposable was disposed; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryDisposeAndNull<T>(ref T? disposable) where T : class, IDisposable
    {
        var original = disposable;
        if (original == null) return false;

        disposable = null;
        original.Dispose();
        return true;
    }

    #endregion
}

internal static partial class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs IsDisposedPropertyChanged = new(nameof(Disposable.IsDisposed));
    internal static readonly PropertyChangedEventArgs IsDisposingPropertyChanged = new(nameof(Disposable.IsDisposing));
}
