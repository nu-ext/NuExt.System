using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel;

/// <summary>
/// Defines a contract for objects whose properties can be accessed by name.
/// </summary>
public interface IReflectableProperties
{
    /// <summary>
    /// Attempts to get the type of the named property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyType">The type of the property when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the property is known; otherwise, <see langword="false"/>.</returns>
    bool TryGetPropertyType(string propertyName, [NotNullWhen(true)] out Type? propertyType);

    /// <summary>
    /// Attempts to get the value of the named property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value of the property when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the value was read; otherwise, <see langword="false"/>.</returns>
    bool TryGetProperty(string propertyName, out object? value);

    /// <summary>
    /// Attempts to assign a value to the named property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to assign.</param>
    /// <returns><see langword="true"/> if the value was assigned; otherwise, <see langword="false"/>.</returns>
    bool TrySetProperty(string propertyName, object? value);
}
