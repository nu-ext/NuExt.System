using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Threading;

/// <summary>
/// A <see cref="SynchronizationContext"/> implementation that adds thread affinity checking
/// and implements <see cref="IThreadAffineSynchronizationContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class wraps an existing synchronization context while providing thread affinity tracking
/// through the <see cref="CheckAccess"/> and <see cref="VerifyAccess"/> methods.
/// </para>
/// <para>
/// It's particularly useful for adapting standard synchronization contexts (like those from WPF or WinForms)
/// to work with APIs requiring <see cref="IThreadAffineSynchronizationContext"/>.
/// </para>
/// </remarks>
public partial class ThreadAffineSynchronizationContext : SynchronizationContext, IThreadAffineSynchronizationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadAffineSynchronizationContext"/> class.
    /// </summary>
    /// <param name="innerContext">The synchronization context to wrap.</param>
    /// <param name="associatedThread">The thread on which <paramref name="innerContext"/> executes its callbacks.</param>
    /// <remarks>
    /// The caller must ensure that <paramref name="innerContext"/> dispatches callbacks to
    /// <paramref name="associatedThread"/>. This relationship cannot be verified by this type.
    /// Supplying a different thread causes <see cref="CheckAccess"/> and <see cref="VerifyAccess"/>
    /// to disagree with the actual execution context used by <see cref="Send"/> and <see cref="Post"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="innerContext"/> is <see langword="null"/> or <paramref name="associatedThread"/> is <see langword="null"/>.</exception>
    public ThreadAffineSynchronizationContext(SynchronizationContext innerContext, Thread associatedThread)
    {
        ArgumentNullException.ThrowIfNull(innerContext);
        ArgumentNullException.ThrowIfNull(associatedThread);
        InnerContext = innerContext;
        Thread = associatedThread;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadAffineSynchronizationContext"/> class.
    /// </summary>
    /// <param name="innerContext">The synchronization context to wrap.</param>
    /// <remarks>
    /// The current thread is associated with <paramref name="innerContext"/>. The caller must ensure
    /// that the context executes its callbacks on that thread.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="innerContext"/> is <see langword="null"/>.</exception>
    public ThreadAffineSynchronizationContext(SynchronizationContext innerContext) : this(innerContext, Thread.CurrentThread)
    {
    }

    /// <inheritdoc/>
    public Thread Thread { get; }

    /// <summary>
    /// Gets the wrapped synchronization context.
    /// </summary>
    public SynchronizationContext InnerContext { get; }

    /// <inheritdoc/>
    public bool CheckAccess()
    {
        return Thread.CurrentThread == Thread;
    }

    /// <inheritdoc/>
    public void VerifyAccess()
    {
        if (!CheckAccess())
        {
            ThrowVerifyAccess();
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ThrowVerifyAccess()
    {
        throw new InvalidOperationException(string.Format(SR.InvalidOperation_SynchronizationContextAccessDenied, 
            Environment.CurrentManagedThreadId, Thread.ManagedThreadId));
    }

    /// <inheritdoc cref="SynchronizationContext.Send" />
    public override void Send(SendOrPostCallback d, object? state)
    {
        InnerContext.Send(d, state);
    }

    /// <inheritdoc cref="SynchronizationContext.Post"/>
    public override void Post(SendOrPostCallback d, object? state)
    {
        InnerContext.Post(d, state);
    }

    /// <inheritdoc/>
    public override SynchronizationContext CreateCopy()
    {
        return new ThreadAffineSynchronizationContext(InnerContext.CreateCopy(), Thread);
    }

    /// <inheritdoc/>
    public override int Wait(nint[] waitHandles, bool waitAll, int millisecondsTimeout)
    {
        return InnerContext.Wait(waitHandles, waitAll, millisecondsTimeout);
    }

    /// <summary>
    /// Creates a <see cref="ThreadAffineSynchronizationContext"/> from the current context.
    /// </summary>
    /// <returns>
    /// A new <see cref="ThreadAffineSynchronizationContext"/> wrapping <see cref="SynchronizationContext.Current"/>,
    /// or <see langword="null"/> if no current context exists.
    /// </returns>
    public static ThreadAffineSynchronizationContext? FromCurrent()
    {
        var current = Current;
        return current != null ? new ThreadAffineSynchronizationContext(current) : null;
    }

    /// <summary>
    /// Creates a <see cref="ThreadAffineSynchronizationContext"/> from the current context
    /// with the specified thread association.
    /// </summary>
    /// <param name="associatedThread">The thread to associate with the context</param>
    /// <returns>
    /// A new <see cref="ThreadAffineSynchronizationContext"/> wrapping <see cref="SynchronizationContext.Current"/>,
    /// or <see langword="null"/> if no current context exists.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="associatedThread"/> is <see langword="null"/>.
    /// </exception>
    public static ThreadAffineSynchronizationContext? FromCurrent(Thread associatedThread)
    {
        ArgumentNullException.ThrowIfNull(associatedThread);
        var current = Current;
        return current != null ? new ThreadAffineSynchronizationContext(current, associatedThread) : null;
    }

    /// <summary>
    /// Creates a <see cref="ThreadAffineSynchronizationContext"/> from the current context
    /// or a default context if none exists.
    /// </summary>
    /// <param name="defaultContext">The default context to use if no current context exists.</param>
    /// <returns>A new <see cref="ThreadAffineSynchronizationContext"/>.</returns>
    /// <remarks>
    /// The selected context is associated with the current thread. If <paramref name="defaultContext"/>
    /// is selected, the caller must ensure that it executes callbacks on the current thread.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="defaultContext"/> is <see langword="null"/>.
    /// </exception>
    public static ThreadAffineSynchronizationContext FromCurrentOrDefault(SynchronizationContext defaultContext)
    {
        ArgumentNullException.ThrowIfNull(defaultContext);
        var context = Current ?? defaultContext;
        return new ThreadAffineSynchronizationContext(context);
    }

    /// <summary>
    /// Creates a <see cref="ThreadAffineSynchronizationContext"/> from the current context
    /// or a default context if none exists.
    /// </summary>
    /// <param name="defaultContext">The default context to use if no current context exists.</param>
    /// <param name="associatedThread">The thread on which the selected context executes its callbacks.</param>
    /// <returns>A new <see cref="ThreadAffineSynchronizationContext"/>.</returns>
    /// <remarks>
    /// The caller must ensure that the selected context dispatches callbacks to
    /// <paramref name="associatedThread"/>. This relationship cannot be verified by this type.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="defaultContext"/> or <paramref name="associatedThread"/> is <see langword="null"/>.
    /// </exception>
    public static ThreadAffineSynchronizationContext FromCurrentOrDefault(SynchronizationContext defaultContext, Thread associatedThread)
    {
        ArgumentNullException.ThrowIfNull(defaultContext);
        ArgumentNullException.ThrowIfNull(associatedThread);
        var context = Current ?? defaultContext;
        return new ThreadAffineSynchronizationContext(context, associatedThread);
    }

    /// <inheritdoc/>
    public override void OperationStarted()
    {
        InnerContext.OperationStarted();
    }

    /// <inheritdoc/>
    public override void OperationCompleted()
    {
        InnerContext.OperationCompleted();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"ThreadAffineSynchronizationContext(Inner={InnerContext.GetType().Name}, " +
               $"Thread={Thread.ManagedThreadId})";
    }
}
