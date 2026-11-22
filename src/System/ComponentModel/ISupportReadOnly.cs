namespace System.ComponentModel;

/// <summary>
/// Defines a contract for objects that can switch to a read-only mode in which modifications are rejected.
/// </summary>
/// <remarks>
/// The state is a mode, not a counter: <see cref="BeginReadOnly"/> and <see cref="EndReadOnly"/> must be
/// paired by the caller. Nested calls are not tracked, so the innermost <see cref="EndReadOnly"/> makes
/// the object writable again.
/// </remarks>
public interface ISupportReadOnly
{
    /// <summary>
    /// Gets a value indicating whether modifications are currently rejected.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Switches the object to a read-only state so that modifications are rejected.
    /// </summary>
    void BeginReadOnly();

    /// <summary>
    /// Switches the object back to a writable state.
    /// </summary>
    void EndReadOnly();
}
