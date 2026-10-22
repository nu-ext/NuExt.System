using System.Runtime.CompilerServices;

namespace System.Threading;

/// <summary>
/// Provides static methods for obtaining locks associated with reference-type keys.
/// </summary>
public static class LockPool
{
    /// <summary>
    /// Gets the <see cref="Lock"/> instance uniquely associated with the specified key object instance.
    /// The same lock instance is returned for the same key object instance across all threads and calls.
    /// Key equality and hash codes are not used. The lifetime of the returned lock is automatically
    /// managed and tied to the lifetime of the key object.
    /// </summary>
    /// <typeparam name="T">The type of the key object.</typeparam>
    /// <param name="key">The key object to get the associated lock for.</param>
    /// <returns>The <see cref="Lock"/> instance associated with <paramref name="key"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method uses the shared instance of <see cref="LockPool{T}"/> (<see cref="LockPool{T}.Shared"/>).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Lock Get<T>(T key) where T : class
    {
        return LockPool<T>.Shared.Get(key);
    }
}

/// <summary>
/// Provides a base contract for pools that manage <see cref="Lock"/> instances associated with reference-type keys.
/// </summary>
/// <typeparam name="T">The type of the key object. Must be a reference type.</typeparam>
/// <remarks>
/// Derived classes define the strategy for associating locks with keys and managing their lifetime.
/// The static <see cref="Shared"/> property provides a default, commonly-used pool instance.
/// </remarks>
public abstract class LockPool<T> where T : class
{
    /// <summary>
    /// Gets the shared, process-wide instance of the lock pool.
    /// For this shared pool, the same lock instance is returned for the same key object instance,
    /// regardless of key equality or hash codes, and its lifetime is tied to the lifetime of the key object.
    /// </summary>
    public static LockPool<T> Shared { get; } = new SharedLockPool<T>();

    /// <summary>
    /// Gets the <see cref="Lock"/> instance associated with the specified key object instance.
    /// </summary>
    /// <param name="key">The key object to get the associated lock for.</param>
    /// <returns>The <see cref="Lock"/> instance associated with <paramref name="key"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public abstract Lock Get(T key);
}

/// <summary>
/// The shared <see cref="LockPool{T}"/> implementation that uses a <see cref="ConditionalWeakTable{TKey, TValue}"/>
/// to associate a unique <see cref="Lock"/> instance with each key object instance for its lifetime.
/// Key equality and hash codes are not used.
/// </summary>
/// <typeparam name="T">The type of the key object.</typeparam>
internal sealed class SharedLockPool<T> : LockPool<T> where T : class
{
    private readonly ConditionalWeakTable<T, Lock> _lockTable = new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Lock Get(T key)
    {
        if (key is null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
        }
        return _lockTable.GetValue(key, static _ => new Lock());
    }
}
