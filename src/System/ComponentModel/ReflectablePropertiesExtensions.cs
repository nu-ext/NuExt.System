namespace System.ComponentModel;

/// <summary>
/// Provides strict accessors for <see cref="IReflectableProperties"/> that throw when a property is unknown or cannot be set.
/// </summary>
public static class ReflectablePropertiesExtensions
{
    /// <summary>
    /// Gets the type of the named property.
    /// </summary>
    /// <param name="source">The object to read the property metadata from.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The type of the property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The property is unknown.</exception>
    public static Type GetPropertyType(this IReflectableProperties source, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!source.TryGetPropertyType(propertyName, out Type? propertyType))
        {
            throw new ArgumentException($"Property '{propertyName}' is not defined.", nameof(propertyName));
        }

        return propertyType;
    }

    /// <summary>
    /// Gets the value of the named property.
    /// </summary>
    /// <param name="source">The object to read the property value from.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The value of the property, which may be <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The property is unknown.</exception>
    public static object? GetProperty(this IReflectableProperties source, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!source.TryGetProperty(propertyName, out object? value))
        {
            throw new ArgumentException($"Property '{propertyName}' is not defined.", nameof(propertyName));
        }

        return value;
    }

    /// <summary>
    /// Assigns a value to the named property.
    /// </summary>
    /// <param name="target">The object to write the property value to.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to assign.</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The property is unknown or its value cannot be assigned.</exception>
    public static void SetProperty(this IReflectableProperties target, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (!target.TrySetProperty(propertyName, value))
        {
            throw new ArgumentException($"Property '{propertyName}' cannot be set.", nameof(propertyName));
        }
    }
}
