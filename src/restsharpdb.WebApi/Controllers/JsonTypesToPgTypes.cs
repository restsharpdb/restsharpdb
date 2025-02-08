using System.Text.Json;

namespace restsharpdb.WebApi.Controllers;

public static class JsonTypesToPgTypes
{
    public static object? ConvertToDbType(this JsonElement element, string postgresType)
    {
        if (element.ValueKind == JsonValueKind.Null)
            return null!;

        
        return postgresType.ToLower() switch
        {
            // Números inteiros
            
            "smallint" => element.TryGetInt32(out var int32Value) ? (short)int32Value : null,
            "integer" or "int4" => element.TryGetInt32(out var intValue) ? intValue : null,
            "bigint" => element.TryGetInt64(out var longValue) ? longValue : null,

            // Números decimais
            "numeric" or "decimal" => element.TryGetDecimal(out var decimalValue) ? decimalValue : null,
            "real"  => element.TryGetDouble(out var doubleValue) ? (float)doubleValue : null,
            "double precision" => element.TryGetDouble(out var dblValue) ? dblValue : null,

            // Texto
            "character varying" or "varchar" or "text" or "char" or "character" =>
                element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),

            // Booleano
            "boolean" or "false" or "true" or "bool" => element.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(element.GetString(), out var bValue) ? bValue : null,
                JsonValueKind.Number => element.GetInt32() != 0,
                _ => null
            },

            // Data e Hora
            "date" => element.ValueKind == JsonValueKind.String &&
                      DateTime.TryParse(element.GetString(), out var dateValue)
                ? dateValue.Date.ToUniversalTime()
                : null,

            "time" or "time without time zone" =>
                element.ValueKind == JsonValueKind.String &&
                TimeSpan.TryParse(element.GetString(), out var timeValue)
                    ? timeValue
                    : null,

            "timestamp" or "timestamp without time zone" =>
                element.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(element.GetString(), out var timestampValue)
                    ? timestampValue.ToUniversalTime()
                    : null,

            "timestamp with time zone"  or "timestamptz" =>
                element.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(element.GetString(), out var timestampTzValue)
                    ? timestampTzValue.ToUniversalTime()
                    : null,

            "interval" => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),

            // Arrays
            //{ } t when t.EndsWith("[]") => ConvertArray(element),

            // JSON
            "json" or "jsonb" => element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetRawText(),

            // UUID
            "uuid" => element.ValueKind switch
            {
                 JsonValueKind.String when Ulid.TryParse(element.GetString(), out var guidValue) => guidValue,
                 JsonValueKind.String when Guid.TryParse(element.GetString(), out var guidValue) => guidValue,
                _ => null
            },

            // Network Types
            "inet" or "cidr" or "macaddr" or "macaddr8" =>
                element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),

            // Geometric Types
            "point" or "line" or "lseg" or "box" or "path" or "polygon" or "circle" =>
                element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),

            // Range Types
            { } t when t.StartsWith("int4range") ||
                       t.StartsWith("int8range") ||
                       t.StartsWith("numrange") ||
                       t.StartsWith("tsrange") ||
                       t.StartsWith("tstzrange") ||
                       t.StartsWith("daterange") =>
                element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),

            // Binary Data
            "bytea" => element.ValueKind == JsonValueKind.String
                ? Convert.FromBase64String(element.GetString()!)
                : null,

            // Enum Type
            { } t when t.StartsWith("enum_") =>
                element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),

            // XML
            "xml" => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),

            // Money
            "money" => element.TryGetDecimal(out var moneyValue) ? moneyValue : null,

            // Bit String
            "bit" or "bit varying" => element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : element.ToString(),

            // Default case
            _ => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString()
        };
    }

}