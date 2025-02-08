using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using PostgrestSharp.Abstract;
using PostgrestSharp.Abstract.DTOs;
using PostgrestSharp.Abstract.Exceptions;
using PostgrestSharp.Abstract.Interfaces.Services;
using PostgrestSharp.WebApi.Controllers;

namespace PostgrestSharp.Business.Services;

public class JsonObjectHandlerService(
    [FromServices] NpgsqlConnection dbConnection,
    [FromServices] IHttpContextAccessor httpContext,
    [FromServices] ILogger<JsonObjectHandlerService> logger,
    [FromServices] ICacheService cacheService) : IJsonObjectHandlerService
{
    private List<TableMetadata>? _tableMetadata;
    private List<RelationshipMetadata>? _relationsMetadata;


    /// <summary>
    /// Persists an object in database. 
    /// </summary>
    /// <param name="tableName">The table where the object will be inserted</param>
    /// <param name="element">The Object to be inserted</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="AlreadyExistsException"></exception>
    public Guid PostMethodHandler(string tableName, JsonElement element)
    {
        _tableMetadata = cacheService.GetOrCreateTableMetadata(tableName, dbConnection)
                         ?? throw new NullReferenceException("Table metadata not found");


        _relationsMetadata = cacheService.GetOrCreateRelationsMetadata(tableName, dbConnection);

        using var comm = dbConnection.CreateCommand();

        var fieldValues = GetFieldValues(element);

        var requiredFields = GetRequiredFields();

        if (requiredFields.Length > 0)
        {
            var missingElements = GetMissingElements(fieldValues, requiredFields);
            if (missingElements.Length != 0)
            {
                throw new ArgumentException(
                    $"The following field(s) cannot be null: {string.Join(",", missingElements)}");
            }
        }

        // Gets the field name marked as primary key
        var pkFieldName = _tableMetadata.SingleOrDefault(s => s.IsPrimaryKey)?.ColumnName
                          ?? throw new NullReferenceException("Primary key not found in tableMetadata");

        //Check if the primary is present in sent object
        var isPkSet = fieldValues.Any(x =>
            x.Name.Equals(pkFieldName, StringComparison.InvariantCultureIgnoreCase));


        //If primary is set, then we need to check if already exists in database
        if (isPkSet)
        {
            comm.CommandText = $"select * from {tableName} where {pkFieldName} = @{pkFieldName}";
            var tmpValue = fieldValues.SingleOrDefault(s => s.Name == pkFieldName)?.RawValue;
            comm.Parameters.AddWithValue($"@{pkFieldName}", tmpValue!);
            var existing = comm.ExecuteScalar();
            if (existing is not null) throw new AlreadyExistsException($"Object Id Already Exists. ID {existing} ");
            comm.Parameters.Clear();
        }

        var pkValue = Guid.Empty;

        //TODO: think about this. If we set a value or throw a exeption, we should return a bad request.This can be a configuration
        //TODO: This can be a configuration to, like accept or not a primary key not set.
        //Just thinking about this
        if (!isPkSet) pkValue = Ulid.NewUlid().ToGuid();

        //the value is discarded but is created in the context of the array of field and values, this
        //ensures the creation
        _ = fieldValues.Select(x => comm.Parameters.AddWithValue($"@{x.Name}", (x.RawValue ?? null)!))
            .ToArray();


        //Adds the previously calculated primary key value
        comm.Parameters.AddWithValue($"@{pkFieldName}", pkValue);

        comm.CommandText =
            $"insert into {tableName} ({string.Join(",", fieldValues.Select(x => x.Name))},{pkFieldName}) values" +
            $" ({string.Join(",", comm.Parameters.Select(x => x.ParameterName).ToArray())})";

        comm.ExecuteNonQuery();
        return pkValue;
    }

    ///<summary>
    /// Retrieves the values of the fields in the input JSON object.
    ///<param name="element">The input JSON object</param>
    /// </summary>
    ///<returns>The values of the fields in the input JSON object</returns>
    private List<JsonFieldValue> GetFieldValues(JsonElement element)
    {
        return element.EnumerateObject().Select(x => new JsonFieldValue
        {
            Name = x.Name.ToLowerInvariant(),
            Value = x.Value,
            PgDataType = GetDataTypeFromMetaData(x.Name)
        }).ToList();
    }

    /// <summary>
    ///  Retrieves the names of the required fields for this table.
    /// </summary>
    /// <returns>The names of the required fields for this table</returns>
    private string[] GetRequiredFields()
    {
        var requiredFields = _tableMetadata!
            .Where(x => x is { IsNullable: false, IsPrimaryKey: false })
            .Select(x => x.ColumnName)
            .ToArray();
        return requiredFields;
    }

    /// <summary>
    ///Retrieves the data type of field based on its name.
    /// </summary>
    ///    <param name="fieldName">The name of the field</param>
    ///    <returns>The data type of the field</returns> 
    private string GetDataTypeFromMetaData(string fieldName)
    {
        return _tableMetadata!
            .First(y => y.ColumnName
                .Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
            .ColumnDataType.ToLowerInvariant();
    }

    ///<summary>
    ///Retrieves the names of any fields in the input JSON object that are missing.
    ///</summary>
    ///<param name="largerArray">The array of field values to compare against</param>
    ///<param name="smallerArray">The array of required field names</param>
    ///<returns>Thenames of any missing fields</returns>
    private static string[] GetMissingElements(IEnumerable<JsonFieldValue> largerArray, string[] smallerArray)
    {
        var namesHashSet = new HashSet<string>(largerArray.Select(field => field.Name));
        return smallerArray.Where(element => !namesHashSet.Contains(element)).ToArray();
    }
}