using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class DatabaseSchemaProvider : IDatabaseSchemaProvider
{
    private readonly string _connectionString;

    public DatabaseSchemaProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<IReadOnlyList<DatabaseTableSchema>> SearchTablesAsync(
        string? table,
        string? keyword,
        int limit,
        bool includeColumns,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                table_schema,
                table_name,
                table_type,
                engine,
                table_rows,
                create_time,
                update_time,
                table_comment
            FROM information_schema.tables
            WHERE table_schema = DATABASE()
              AND (@TableName IS NULL OR table_name = @TableName)
              AND (
                    @Keyword IS NULL
                    OR table_name LIKE CONCAT('%', @Keyword, '%')
                    OR table_comment LIKE CONCAT('%', @Keyword, '%')
                  )
            ORDER BY table_name
            LIMIT @Limit;
            """;

        await using var connection = new MySqlConnection(_connectionString);

        var tables = (await connection.QueryAsync<DatabaseTableSchema>(
            new CommandDefinition(
                sql,
                new
                {
                    TableName = string.IsNullOrWhiteSpace(table) ? null : table.Trim(),
                    Keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim(),
                    Limit = limit
                },
                cancellationToken: cancellationToken))).AsList();

        if (includeColumns && tables.Count > 0)
        {
            await PopulateColumnsAsync(connection, tables, cancellationToken);
        }

        return tables;
    }

    public async Task<DatabaseTableSchema?> GetTableAsync(string table, CancellationToken cancellationToken = default)
    {
        var tables = await SearchTablesAsync(table, keyword: null, limit: 1, includeColumns: true, cancellationToken);

        return tables.FirstOrDefault();
    }

    private static async Task PopulateColumnsAsync(
        MySqlConnection connection,
        IReadOnlyList<DatabaseTableSchema> tables,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                table_name,
                column_name,
                ordinal_position,
                data_type,
                column_type,
                is_nullable,
                column_key,
                column_default,
                extra,
                column_comment
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
              AND table_name IN @TableNames
            ORDER BY table_name, ordinal_position;
            """;

        var tableNames = tables.Select(static table => table.TableName).ToArray();
        var columns = (await connection.QueryAsync<DatabaseColumnSchema>(
            new CommandDefinition(
                sql,
                new { TableNames = tableNames },
                cancellationToken: cancellationToken))).AsList();

        var columnsByTable = columns
            .GroupBy(static column => column.TableName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static group => group.Key, static group => (IReadOnlyList<DatabaseColumnSchema>)group.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var table in tables)
        {
            table.Columns = columnsByTable.TryGetValue(table.TableName, out var tableColumns)
                ? tableColumns
                : [];
        }
    }
}
