namespace System.ComponentModel;

/// <summary>
/// Defines a contract for objects that require one-time synchronous initialization.
/// </summary>
/// <remarks>
/// <para>
/// Initialization is <em>idempotent</em>: subsequent calls to <see cref="Initialize"/> after a successful
/// initialization complete immediately without repeating the initialization work, and
/// <see cref="IsInitialized"/> is <see langword="true"/>.
/// </para>
/// <para>
/// A failed or canceled initialization must leave the object uninitialized: <see cref="IsInitialized"/> stays
/// <see langword="false"/> and a later call is allowed to retry the initialization. Implementations must not
/// cache the failure, so initializing through <see cref="Lazy{T}"/> or <see cref="System.Threading.AsyncLazy{T}"/> in their default
/// mode is not suitable: they re-throw the cached exception on every subsequent access.
/// </para>
/// <para>
/// Thread-affinity: implementations may require being invoked on a specific thread (for example, the UI thread).
/// Such requirements should be documented by the implementing type.
/// </para>
/// <para>
/// This contract describes the lifecycle of the object itself. It is unrelated to
/// <see cref="ISupportInitialize"/>, which suspends and resumes property change notifications during databinding.
/// </para>
/// </remarks>
public interface IInitializable
{
    /// <summary>
    /// Gets a value indicating whether the instance has been successfully initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the instance.
    /// </summary>
    /// <remarks>
    /// Calling this method on an already initialized instance does nothing.
    /// </remarks>
    void Initialize();
}
