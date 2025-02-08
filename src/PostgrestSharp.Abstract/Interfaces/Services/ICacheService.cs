using Npgsql;

namespace PostgrestSharp.Abstract.Interfaces.Services;


public interface ICacheService
{
    /// <summary>
    /// Retrieves the relationship from database and returns to the cache
    /// </summary>
    /// <param name="tableName">Table Name</param>
    /// <param name="connection">The Npgsql connection</param>
    /// <returns></returns>
    List<RelationshipMetadata>? GetOrCreateRelationsMetadata(string tableName, NpgsqlConnection connection);

    /// <summary>
    /// Retrieves the table metadata and stores in the cache.
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="connection">The database connection</param>
    /// <returns></returns>
    List<TableMetadata>? GetOrCreateTableMetadata(string tableName, NpgsqlConnection connection);

   
}