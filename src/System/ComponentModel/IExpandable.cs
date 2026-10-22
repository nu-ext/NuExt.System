namespace System.ComponentModel;

/// <summary>
/// Defines a contract for objects that can be expanded and collapsed.
/// </summary>
public interface IExpandable
{
    /// <summary>
    /// Gets or sets a value indicating whether the object is expanded.
    /// </summary>
    bool IsExpanded { get; set; }

#if NETSTANDARD2_1_OR_GREATER || NET
    /// <summary>
    /// Collapses the object.
    /// </summary>
    /// <remarks>
    /// The default implementation sets <see cref="IsExpanded"/> to <see langword="false"/>.
    /// </remarks>
    void Collapse()
    {
        IsExpanded = false;
    }
#else
    /// <summary>
    /// Collapses the object.
    /// </summary>
    void Collapse();
#endif

#if NETSTANDARD2_1_OR_GREATER || NET
    /// <summary>
    /// Expands the object.
    /// </summary>
    /// <remarks>
    /// The default implementation sets <see cref="IsExpanded"/> to <see langword="true"/>.
    /// </remarks>
    void Expand()
    {
        IsExpanded = true;
    }
#else
    /// <summary>
    /// Expands the object.
    /// </summary>
    void Expand();
#endif
}