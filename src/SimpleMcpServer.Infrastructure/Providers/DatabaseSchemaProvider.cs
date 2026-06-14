using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.Models;
using SimpleMcpServer.Domain.Entities;
using SimpleMcpServer.Infrastructure.Options;

namespace SimpleMcpServer.Infrastructure.Providers;

public class DatabaseSchemaProvider : IDatabaseSchemaProvider
{
    private readonly IConfiguration _configuration;
    private readonly DatabaseSchemaOptions _options;

    public DatabaseSchemaProvider(IConfiguration configuration)
    {
        _configuration = configuration;
        _options = DatabaseSchemaOptions.FromConfiguration(configuration);
    }

    public Task<IReadOnlyList<DatabaseSchemaTargetInfo>> ListTargetsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DatabaseSchemaTargetInfo> targets = _options
            .Targets
            .OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new DatabaseSchemaTargetInfo
            {
                Alias = pair.Key,
                DisplayName = pair.Value.DisplayName,
                IsDefault = pair.Key.Equals(_options.DefaultTarget, StringComparison.OrdinalIgnoreCase)
            })
            .ToArray();

        return Task.FromResult(targets);
    }

    public async Task<IReadOnlyList<string>> ListDatabasesAsync(
        string target,
        bool includeSystemDatabases,
        CancellationToken cancellationToken = default)
    {
        var alias = NormalizeRequired(target, nameof(target));
        var targetOptions = GetTarget(alias);

        await using var connection = new MySqlConnection(GetConnectionString(alias, targetOptions));
        var databases = await ListDatabasesAsync(connection, includeSystemDatabases, int.MaxValue, cancellationToken);

        return databases;
    }

    public async Task<DatabaseSchemaSearchResult> SearchAsync(
        DatabaseSchemaQuery query,
        CancellationToken cancellationToken = default)
    {
        var targetAliases = ResolveTargets(query);
        var targetResults = new List<DatabaseSchemaTargetResult>();

        foreach (var alias in targetAliases)
        {
            var targetOptions = GetTarget(alias);

            await using var connection = new MySqlConnection(GetConnectionString(alias, targetOptions));
            var currentDatabase = await GetCurrentDatabaseAsync(connection, cancellationToken);
            var databases = await ResolveDatabasesAsync(connection, query, currentDatabase, cancellationToken);
            var databaseResults = new List<DatabaseSchemaDatabaseResult>();

            foreach (var database in databases)
            {
                var tables = await SearchTablesForDatabaseAsync(connection, database, query, cancellationToken);

                if (query.IncludeColumns && tables.Count > 0)
                {
                    await PopulateColumnsAsync(connection, tables, cancellationToken);
                }

                databaseResults.Add(new DatabaseSchemaDatabaseResult
                {
                    Database = database,
                    Tables = tables
                });
            }

            targetResults.Add(new DatabaseSchemaTargetResult
            {
                Target = alias,
                DisplayName = targetOptions.DisplayName,
                CurrentDatabase = currentDatabase,
                Databases = databaseResults
            });
        }

        return new DatabaseSchemaSearchResult
        {
            Targets = targetResults
        };
    }

    private async Task<IReadOnlyList<string>> ResolveDatabasesAsync(
        MySqlConnection connection,
        DatabaseSchemaQuery query,
        string? currentDatabase,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(query.Database))
        {
            var database = query.Database.Trim();

            ValidateDatabaseScope(database, query.IncludeSystemDatabases);

            var exists = await DatabaseExistsAsync(connection, database, cancellationToken);

            return exists ? [database] : [];
        }

        if (query.AllDatabases)
        {
            return await ListDatabasesAsync(
                connection,
                query.IncludeSystemDatabases,
                query.MaxDatabases,
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(currentDatabase))
        {
            throw new InvalidOperationException("The configured database connection does not select a default database.");
        }

        ValidateDatabaseScope(currentDatabase, query.IncludeSystemDatabases);

        return [currentDatabase];
    }

    private async Task<IReadOnlyList<string>> ListDatabasesAsync(
        MySqlConnection connection,
        bool includeSystemDatabases,
        int maxDatabases,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT schema_name
            FROM information_schema.schemata
            WHERE @IncludeSystemDatabases = TRUE
               OR schema_name NOT IN @ExcludedSchemas
            ORDER BY schema_name
            LIMIT @MaxDatabases;
            """;

        var databases = await connection.QueryAsync<string>(
            new CommandDefinition(
                sql,
                new
                {
                    IncludeSystemDatabases = includeSystemDatabases,
                    ExcludedSchemas = _options.ExcludedSchemas.ToArray(),
                    MaxDatabases = maxDatabases
                },
                cancellationToken: cancellationToken));

        return databases.AsList();
    }

    private static async Task<string?> GetCurrentDatabaseAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = "SELECT DATABASE();";

        return await connection.ExecuteScalarAsync<string?>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    private static async Task<bool> DatabaseExistsAsync(
        MySqlConnection connection,
        string database,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM information_schema.schemata
            WHERE schema_name = @Database;
            """;

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new { Database = database },
                cancellationToken: cancellationToken));

        return count > 0;
    }

    private static async Task<IReadOnlyList<DatabaseTableSchema>> SearchTablesForDatabaseAsync(
        MySqlConnection connection,
        string database,
        DatabaseSchemaQuery query,
        CancellationToken cancellationToken)
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
            WHERE table_schema = @Database
              AND (@TableName IS NULL OR table_name = @TableName)
              AND (
                    @Keyword IS NULL
                    OR table_name LIKE CONCAT('%', @Keyword, '%')
                    OR table_comment LIKE CONCAT('%', @Keyword, '%')
                  )
            ORDER BY table_name
            LIMIT @Limit;
            """;

        var tables = await connection.QueryAsync<DatabaseTableSchema>(
            new CommandDefinition(
                sql,
                new
                {
                    Database = database,
                    TableName = string.IsNullOrWhiteSpace(query.Table) ? null : query.Table.Trim(),
                    Keyword = string.IsNullOrWhiteSpace(query.Keyword) ? null : query.Keyword.Trim(),
                    query.Limit
                },
                cancellationToken: cancellationToken));

        return tables.AsList();
    }

    private static async Task PopulateColumnsAsync(
        MySqlConnection connection,
        IReadOnlyList<DatabaseTableSchema> tables,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                table_schema,
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
            WHERE table_schema = @Database
              AND table_name IN @TableNames
            ORDER BY table_schema, table_name, ordinal_position;
            """;

        var columns = new List<DatabaseColumnSchema>();

        foreach (var databaseGroup in tables.GroupBy(static table => table.TableSchema, StringComparer.OrdinalIgnoreCase))
        {
            var tableNames = databaseGroup.Select(static table => table.TableName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            var databaseColumns = await connection.QueryAsync<DatabaseColumnSchema>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Database = databaseGroup.Key,
                        TableNames = tableNames
                    },
                    cancellationToken: cancellationToken));

            columns.AddRange(databaseColumns);
        }

        var columnsByTable = columns
            .GroupBy(static column => $"{column.TableSchema}\u001f{column.TableName}", StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static group => group.Key, static group => (IReadOnlyList<DatabaseColumnSchema>)group.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var table in tables)
        {
            var key = $"{table.TableSchema}\u001f{table.TableName}";
            table.Columns = columnsByTable.TryGetValue(key, out var tableColumns)
                ? tableColumns
                : [];
        }
    }

    private IReadOnlyList<string> ResolveTargets(DatabaseSchemaQuery query)
    {
        if (query.AllTargets)
        {
            return _options
                .Targets
                .Keys
                .OrderBy(alias => alias.Equals(_options.DefaultTarget, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(static alias => alias, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        var aliases = query.TargetAliases is { Count: > 0 }
            ? query.TargetAliases
            : [_options.DefaultTarget];

        var distinctAliases = aliases
            .Select(static alias => NormalizeRequired(alias, "target"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var alias in distinctAliases)
        {
            _ = GetTarget(alias);
        }

        return distinctAliases;
    }

    private DatabaseSchemaTargetOptions GetTarget(string alias)
    {
        if (_options.Targets.TryGetValue(alias, out var target))
        {
            return target;
        }

        throw new InvalidOperationException(
            $"Database schema target '{alias}' is not configured. " +
            $"Allowed targets: {string.Join(", ", _options.Targets.Keys.OrderBy(static key => key, StringComparer.OrdinalIgnoreCase))}.");
    }

    private string GetConnectionString(string alias, DatabaseSchemaTargetOptions target)
    {
        var connectionString = _configuration.GetConnectionString(target.ConnectionStringName);

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException(
                $"Connection string '{target.ConnectionStringName}' for database schema target '{alias}' is not configured.")
            : connectionString;
    }

    private void ValidateDatabaseScope(string database, bool includeSystemDatabases)
    {
        if (!includeSystemDatabases &&
            _options.ExcludedSchemas.Contains(database, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Database schema '{database}' is excluded. Set include_system_databases=true to inspect system schemas.");
        }
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        var normalized = value.Trim();

        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : normalized;
    }
}
