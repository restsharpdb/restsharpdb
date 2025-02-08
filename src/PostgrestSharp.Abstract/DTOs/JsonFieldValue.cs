using System.Text.Json;
using PostgrestSharp.Abstract.Extensions;

namespace PostgrestSharp.Abstract.DTOs;

/// <summary>
/// Represents a field value in JSON format with additional metadata.
/// </summary>
public class JsonFieldValue
{
    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON element representing the value.
    /// </summary>
    public JsonElement Value { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL data type associated with the field.
    /// </summary>
    public string PgDataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets the raw value of the JSON element, converted to its corresponding database type based on the PostgreSQL data type.
    /// </summary>
    public object? RawValue => Value.ConvertToDbType(PgDataType);
}