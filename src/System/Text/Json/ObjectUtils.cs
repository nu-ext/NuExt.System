using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json;

/// <summary>
/// Contains utility methods for working with objects and JSON.
/// </summary>
public static class ObjectUtils
{
    private static readonly JsonDocumentOptions s_documentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Deserializes a JSON string into objects using specified delegates to get the object type and set the value.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="getObjectType">A function to get the object type by property name.</param>
    /// <param name="setValue">A function to set the value of the object by property name.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getObjectType"/> or <paramref name="setValue"/> is <see langword="null"/>.
    /// </exception>
    public static void DeserializeObject(string json, Func<string, Type?> getObjectType, Func<string, object?, bool> setValue)
    {
        DeserializeObject(json, getObjectType, setValue, null, out _);
    }

    /// <summary>
    /// Deserializes a JSON string into objects using specified delegates to get the object type and set the value.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="getObjectType">A function to get the object type by property name.</param>
    /// <param name="setValue">A function to set the value of the object by property name.</param>
    /// <param name="options">Options for JSON deserialization.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getObjectType"/> or <paramref name="setValue"/> is <see langword="null"/>.
    /// </exception>
    public static void DeserializeObject(string json, Func<string, Type?> getObjectType, Func<string, object?, bool> setValue, JsonSerializerOptions? options)
    {
        DeserializeObject(json, getObjectType, setValue, options, out _);
    }

    /// <summary>
    /// Deserializes a JSON string into objects using specified delegates to get the object type and set the value.
    /// Any properties that cannot be matched to a known type are stored in additionalData.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="getObjectType">A function to get the object type by property name.</param>
    /// <param name="setValue">A function to set the value of the object by property name.</param>
    /// <param name="options">Options for JSON deserialization.</param>
    /// <param name="additionalData">A dictionary to store any additional data that does not match known types.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getObjectType"/> or <paramref name="setValue"/> is <see langword="null"/>.
    /// </exception>
    public static void DeserializeObject(string json, Func<string, Type?> getObjectType, Func<string, object?, bool> setValue, JsonSerializerOptions? options, out Dictionary<string, JsonElement>? additionalData)
    {
        ArgumentNullException.ThrowIfNull(getObjectType);
        ArgumentNullException.ThrowIfNull(setValue);

        additionalData = null;
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        //var json = JsonSerializer.Deserialize<JsonDocument>(json);
        try
        {
            using var document = JsonDocument.Parse(json, s_documentOptions);
            JsonElement root = document.RootElement;
            foreach (JsonProperty property in root.EnumerateObject())
            {
                string name = property.Name;
                var value = property.Value;
                var type = getObjectType(name);
                if (type == null)
                {
                    additionalData ??= [];
                    additionalData[name] = value.Clone();
                    continue;
                }
                try
                {
                    var obj = value.Deserialize(type, options);
                    if (!setValue(name, obj))
                    {
                        Trace.WriteLine($"Can't set property: {name}");
                        Debug.Fail($"Can't set property: {name}");
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Failed to deserialize property '{name}': {ex}");
                    Debug.Fail($"Failed to deserialize property '{name}': {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to deserialize JSON object: {ex}");
            Debug.Fail($"Failed to deserialize JSON object: {ex.Message}");
        }
    }

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="source">The object to serialize.</param>
    /// <returns>A JSON string or null if the object is null.</returns>
    public static string? SerializeObject<T>(T source)
    {
        return SerializeObject(source, null);
    }

    /// <summary>
    /// Serializes an object to a JSON string using specified options.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="source">The object to serialize.</param>
    /// <param name="options">Options for JSON serialization.</param>
    /// <returns>A JSON string or null if the object is null.</returns>
    public static string? SerializeObject<T>(T source, JsonSerializerOptions? options)
    {
        if (source is null)
        {
            return null;
        }
        var type = source.GetType();
        var value = JsonSerializer.Serialize(source, type, options);
        return value;
    }
}
