using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PostgrestSharp.Abstract.DTOs;
using PostgrestSharp.Abstract.Interfaces.Services;

namespace restsharpdb.WebApi.Controllers;

[ApiController]
[Route("api/object")]
public class DynamicController(
    IHttpContextAccessor context,
    ILogger<DynamicController> logger,
    IJsonObjectHandlerService objectHandlerService
)
    : ControllerBase
{
    /// <summary>
    /// Inserts one or more elements into a table.
    /// </summary>
    /// <param name="tableName">target table</param>
    /// <param name="body">The elements to be inserted. Can be one or more elements.</param>
    /// <returns cref="SuccessResponse">The response with created ids.</returns>
    [HttpPost("{tableName}")]
    public async Task<object> PostTable([FromRoute] string tableName,
        [FromBody] JsonDocument body)
    {
        //TODO: Check if table exists before continue
        //TODO: think about data validation, use the table metadata or just send the element to database to validate against the table
        logger.LogDebug("Inserting {valueKind} into '{tableName}'", body.RootElement.ValueKind, tableName);
        
        if (string.IsNullOrWhiteSpace(tableName)) return BadRequest("tableName parameter is null or empty");

        var results = new SuccessResponse(tableName);
        
        foreach (var element in GetOneOrManyElements(body))
        {
            results.ids.Add(objectHandlerService.PostMethodHandler(tableName, element));
        }

        return Ok(results);
    }


    [HttpDelete("{tableName}/{objId}")]
    public object DeleteFromTable([FromRoute] string tableName, [FromRoute] Guid? objId = null)
    {
         
        
        
        return Ok();
    }

    [HttpPut("{tableName}")]
    public object PutTable([FromRoute] string tableName, [FromBody] JsonDocument body)
    {
        //TODO: Check if table exists before continue
        logger.LogDebug("Updating {valueKind} into '{tableName}'", body.RootElement.ValueKind, tableName);
        if (string.IsNullOrWhiteSpace(tableName)) return BadRequest("tableName parameter is null or empty");

        var results = new SuccessResponse(tableName);

        foreach (var element in GetOneOrManyElements(body))
        {
            results.ids.Add(objectHandlerService.PostMethodHandler(tableName, element));
        }

        return Ok(results);
    }


    /// <summary>
    /// Creates a sortable id. The id is chronologically sortable and unique.
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetId")]
    public Guid GetId()
    {
        return Ulid.NewUlid().ToGuid();
    }

    /// <summary>
    /// Get the elements of the body of request
    /// </summary>
    /// <param name="body">A json element to store at the database</param>
    /// <returns></returns>
    private static List<JsonElement> GetOneOrManyElements(JsonDocument body)
    {
        var elements = new List<JsonElement>();
        elements.AddRange(body.RootElement.ValueKind == JsonValueKind.Array
            ? body.RootElement.EnumerateArray()
            : [body.RootElement]);
        return elements;
    }
}