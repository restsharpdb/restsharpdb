using System.Text.Json;
using PostgrestSharp.Abstract.DTOs;

namespace PostgrestSharp.Abstract.Interfaces.Services;

public interface IJsonObjectHandlerService
{
    Guid PostMethodHandler(string tableName, JsonElement element);
}