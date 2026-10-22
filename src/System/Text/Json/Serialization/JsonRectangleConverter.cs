using System.Diagnostics;
using System.Drawing;

namespace System.Text.Json.Serialization;

/// <summary>
/// Custom JSON converter for the <see cref="Rectangle"/> struct. This converter handles serialization and deserialization
/// of <see cref="Rectangle"/> instances to and from JSON objects.
/// </summary>
[DebuggerStepThrough]
public sealed class JsonRectangleConverter : JsonConverter<Rectangle>
{
    /// <summary>
    /// Reads a JSON object representing a <see cref="Rectangle"/> and converts it into an instance of the <see cref="Rectangle"/> struct.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">Options to control the conversion behavior.</param>
    /// <returns>An instance of <see cref="Rectangle"/> deserialized from the JSON data.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is not in the expected format.</exception>
    public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        int x = 0, y = 0, w = 0, h = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Rectangle(x, y, w, h);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();
            if (!reader.Read())
            {
                throw new JsonException();
            }

            switch (propertyName)
            {
                case nameof(Rectangle.X):
                    x = reader.GetInt32();
                    break;
                case nameof(Rectangle.Y):
                    y = reader.GetInt32();
                    break;
                case nameof(Rectangle.Width):
                    w = reader.GetInt32();
                    break;
                case nameof(Rectangle.Height):
                    h = reader.GetInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }
        throw new JsonException();
    }

    /// <summary>
    /// Writes an instance of the <see cref="Rectangle"/> struct to a JSON object.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The <see cref="Rectangle"/> value to write.</param>
    /// <param name="options">Options to control the conversion behavior.</param>
    public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(nameof(Rectangle.X), value.X);
        writer.WriteNumber(nameof(Rectangle.Y), value.Y);
        writer.WriteNumber(nameof(Rectangle.Width), value.Width);
        writer.WriteNumber(nameof(Rectangle.Height), value.Height);
        writer.WriteEndObject();
    }
}
