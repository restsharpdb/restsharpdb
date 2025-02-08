using System.Data;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using PostgrestSharp.Abstract;
using PostgrestSharp.Abstract.Enums;
using PostgrestSharp.Abstract.Interfaces.Services;

namespace PostgrestSharp.WebApi.Controllers;

public class CacheService(IMemoryCache cache, MemoryCacheEntryOptions options) : ICacheService
{
    /// <summary>
    /// Retrieves the relationship from database and returns to the cache
    /// </summary>
    /// <param name="tableName">Table Name</param>
    /// <param name="connection">The Npgsql connection</param>
    /// <returns></returns>
    public List<RelationshipMetadata>? GetOrCreateRelationsMetadata(string tableName, NpgsqlConnection connection)
    {
        return cache.GetOrCreate($"{tableName}_relation", entry =>
        {
            using var comm = connection.CreateCommand();
            comm.CommandText = $"select * from get_table_relationships(@table_name)";
            comm.Parameters.AddWithValue("@table_name", tableName);

            using var reader = comm.ExecuteReader();

            var metadata = new List<RelationshipMetadata>();

            while (reader.Read())
            {
                metadata.Add(new RelationshipMetadata
                {
                    SourceTable = reader.GetString("source_table"),
                    TargetTable = reader.GetString("target_table"),
                    FkColumn = reader.GetString("fk_column"),
                    PkColumn = reader.GetString("pk_column"),
                    RelationshipType = Enum.Parse<RelationsipType>(reader.GetString("relationship_type")),

                    JunctionTable = reader.IsDBNull("junction_table")
                        ? null
                        : reader.GetString("junction_table"),

                    TargetFkColumn = reader.IsDBNull("target_fk_column")
                        ? null
                        : reader.GetString("target_fk_column"),

                    TargetPkColumn = reader.IsDBNull("target_pk_column")
                        ? null
                        : reader.GetString("target_pk_column"),

                    RelationshipName = reader.GetString("relationship_name"),
                    IsSselfReferencing = reader.GetBoolean("is_self_referencing"),

                    SelfReferencingAlias = reader.IsDBNull("self_referencing_alias")
                        ? null
                        : reader.GetString("self_referencing_alias")
                });
            }

            reader.Close();
            return metadata;
        }, options);
    }


    /// <summary>
    /// Retrieves the table metadata and stores in the cache.
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="connection">The database connection</param>
    /// <returns></returns>
    public List<TableMetadata>? GetOrCreateTableMetadata(string tableName, NpgsqlConnection connection)
    {
        return cache.GetOrCreate(tableName, entry =>
        {
            using var comm = connection.CreateCommand();
            comm.CommandText = "select * from get_table_metadata(@table_name)";
            comm.Parameters.AddWithValue("@table_name", tableName);

            using var reader = comm.ExecuteReader();

            var metadata = new List<TableMetadata>();
            while (reader.Read())
            {
                metadata.Add(new TableMetadata
                {
                    ColumnName = reader.GetString("column_name"),
                    ColumnDataType = reader.GetString("data_type"),
                    IsPrimaryKey = reader.GetBoolean("is_primary_key"),
                    IsNullable = reader.GetBoolean("is_nullable"),
                    IsSelfReferencing = reader.GetBoolean("is_self_referencing"),
                    MaxLength = !reader.IsDBNull("character_maximum_length")
                        ? reader.GetInt32("character_maximum_length")
                        : null,
                    OrdinalPosition = reader.GetInt32("ordinal_position")
                });
            }

            reader.Close();
            
            return metadata;
        }, options);
    }
}