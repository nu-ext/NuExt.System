using System.Threading;

namespace System;

/// <summary>
/// Provides a lightweight registration point for platform- or application-specific adapters.
/// </summary>
/// <remarks>
/// Use this class to register optimized access-check delegates for known
/// <see cref="SynchronizationContext"/> implementations to avoid reflection or runtime
/// code generation costs.
/// </remarks>
public static class Bootstrap
{
    /// <summary>
    /// Registers an optimized access-check delegate for the synchronization context type
    /// <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TContext">
    /// The concrete <see cref="SynchronizationContext"/> type to register.
    /// </typeparam>
    /// <param name="checkAccess">
    /// A delegate that determines whether the calling thread has access to the specified
    /// <typeparamref name="TContext"/> instance.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="checkAccess"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// If a delegate is already registered for <typeparamref name="TContext"/>, it is replaced.
    /// </remarks>
    public static void RegisterCheckAccessDelegate<TContext>(Func<TContext, bool> checkAccess)
        where TContext : SynchronizationContext
    {
        ArgumentNullException.ThrowIfNull(checkAccess);
        SynchronizationContextHelper.RegisterCheckAccessDelegate(typeof(TContext), ctx => checkAccess((TContext)ctx));
    }

    /// <summary>
    /// Unregisters a previously registered access-check delegate for the synchronization context type
    /// <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TContext">
    /// The concrete <see cref="SynchronizationContext"/> type whose registration should be removed.
    /// </typeparam>
    /// <returns>
    /// <see langword="true"/> if a registration existed and was removed; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool UnregisterCheckAccessDelegate<TContext>()
        where TContext : SynchronizationContext
    {
        return SynchronizationContextHelper.UnregisterCheckAccessDelegate(typeof(TContext));
    }
}