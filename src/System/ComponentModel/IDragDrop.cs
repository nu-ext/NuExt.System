namespace System.ComponentModel;

/// <summary>
/// Defines methods and properties for drag-and-drop functionality.
/// </summary>
public interface IDragDrop
{
    /// <summary>
    /// Gets a value indicating whether the object can be dragged.
    /// </summary>
    bool CanDrag { get; }

    /// <summary>
    /// Determines whether a specified object can be dropped onto this object.
    /// </summary>
    /// <param name="draggedObject">The object being dragged.</param>
    /// <returns><see langword="true"/> if the object can be dropped; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="draggedObject"/> is <see langword="null"/>.</exception>
    bool CanDrop(IDragDrop draggedObject);

    /// <summary>
    /// Executes the drop action for a specified object.
    /// </summary>
    /// <param name="draggedObject">The object being dragged.</param>
    /// <returns><see langword="true"/> if the drop operation was successful; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="draggedObject"/> is <see langword="null"/>.</exception>
    bool Drop(IDragDrop draggedObject);
}