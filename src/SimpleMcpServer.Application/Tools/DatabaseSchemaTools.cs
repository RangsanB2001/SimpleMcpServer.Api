using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.Models;
using SimpleMcpServer.Domain.Entities;
using System.ComponentModel;
using System.Text;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
[McpServerResourceType]
public class DatabaseSchemaTools
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 200;
    private const int DefaultMaxDatabases = 20;
    private const int MaxDatabases = 200;
    private const string SourceSystem = "PSIMS";
    private const string MetadataSource = "MySQL information_schema";
    private readonly IDatabaseSchemaProvider _provider;

    public DatabaseSchemaTools(IDatabaseSchemaProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "database_schema")]
    [Description(
        "Get read-only database schema metadata for configured PSIMS MySQL targets. " +
        "Targets are allowlisted aliases from server configuration; raw IPs and credentials are not accepted. " +
        "Use this to understand available databases, tables, and columns before choosing the right financial data tool. " +
        "Returns metadata from information_schema only; no business rows are returned.")]
    public async Task<object> GetDatabaseSchema(
        [Description("Exact table name to inspect, e.g. Compsec, Company, d_trade. Optional.")] string? table = null,
        [Description("Keyword to match table name or table comment. Ignored when table is provided. Optional.")] string? keyword = null,
        [Description("Maximum tables to return per database, 1-200. Default 50.")] int limit = DefaultLimit,
        [Description("Include column definitions for matched tables. Default true.")] bool include_columns = true,
        [Description("Configured target alias to inspect, e.g. psims or psims-uat. Optional; defaults to configured DefaultTarget.")] string? target = null,
        [Description("Comma-separated configured target aliases, or 'all'. Optional; do not combine with target.")] string? targets = null,
        [Description("Database/schema name to inspect on the selected target(s). Optional; defaults to the connection's current database.")] string? database = null,
        [Description("Inspect all visible databases on selected target(s), excluding system schemas by default. Do not combine with database.")] bool all_databases = false,
        [Description("Allow system schemas such as information_schema, mysql, performance_schema, and sys. Default false.")] bool include_system_databases = false,
        [Description("Maximum databases to inspect per target when all_databases=true, 1-200. Default 20.")] int max_databases = DefaultMaxDatabases,
        CancellationToken cancellationToken = default)
    {
        var boundedLimit = ValidateLimit(limit);
        var boundedMaxDatabases = ValidateMaxDatabases(max_databases);
        var normalizedTable = NormalizeOptional(table);
        var normalizedKeyword = NormalizeOptional(keyword);
        var normalizedDatabase = NormalizeOptional(database);
        var (allTargets, targetAliases) = ParseTargetSelection(target, targets);

        if (normalizedDatabase is not null && all_databases)
        {
            throw new ArgumentException("Specify either database or all_databases, not both.", nameof(all_databases));
        }

        var query = new DatabaseSchemaQuery
        {
            TargetAliases = targetAliases,
            AllTargets = allTargets,
            Database = normalizedDatabase,
            AllDatabases = all_databases,
            IncludeSystemDatabases = include_system_databases,
            Table = normalizedTable,
            Keyword = normalizedKeyword,
            Limit = boundedLimit,
            MaxDatabases = boundedMaxDatabases,
            IncludeColumns = include_columns
        };

        var result = await _provider.SearchAsync(query, cancellationToken);

        if (normalizedTable is not null && result.TableCount == 0)
        {
            throw new InvalidOperationException(
                $"No database schema found for table '{normalizedTable}' in the selected target/database scope.");
        }

        return new
        {
            tool = "database_schema",
            query = new
            {
                target = NormalizeOptional(target),
                targets = NormalizeOptional(targets),
                database = normalizedDatabase,
                all_databases,
                include_system_databases,
                table = normalizedTable,
                keyword = normalizedKeyword,
                limit = boundedLimit,
                max_databases = boundedMaxDatabases,
                include_columns
            },
            source = Source(result),
            target_count = result.Targets.Count,
            database_count = result.DatabaseCount,
            count = result.TableCount,
            targets = result.Targets.Select(targetResult => ToTargetResult(targetResult, include_columns)).ToArray()
        };
    }

    [McpServerResource(
        UriTemplate = "database-schema://tables",
        Name = "database_schema_tables",
        Title = "Database Tables",
        MimeType = "text/markdown")]
    [Description("List tables from the default configured database schema target for AI context.")]
    public async Task<TextResourceContents> GetDatabaseTablesResource(CancellationToken cancellationToken = default)
    {
        var result = await _provider.SearchAsync(DefaultResourceQuery(includeColumns: false), cancellationToken);

        return new TextResourceContents
        {
            Uri = "database-schema://tables",
            MimeType = "text/markdown",
            Text = BuildSchemaMarkdown(result, title: "Database tables")
        };
    }

    [McpServerResource(
        UriTemplate = "database-schema://table/{table}",
        Name = "database_schema_table",
        Title = "Database Table Schema",
        MimeType = "text/markdown")]
    [Description("Get one table schema from the default configured database schema target.")]
    public async Task<TextResourceContents> GetDatabaseTableResource(string table, CancellationToken cancellationToken = default)
    {
        var normalizedTable = NormalizeRequired(table, nameof(table));
        var query = DefaultResourceQuery(includeColumns: true) with
        {
            Table = normalizedTable,
            Limit = 1
        };
        var result = await _provider.SearchAsync(query, cancellationToken);
        var tableResult = FindFirstTable(result);
        var text = tableResult is null
            ? $"# Database table schema: {EscapeMarkdown(normalizedTable)}{Environment.NewLine}{Environment.NewLine}No schema found for this table."
            : BuildTableMarkdown(tableResult.Value.Target, tableResult.Value.Table);

        return new TextResourceContents
        {
            Uri = TableResourceUri(normalizedTable),
            MimeType = "text/markdown",
            Text = text
        };
    }

    [McpServerResource(
        UriTemplate = "database-schema://targets",
        Name = "database_schema_targets",
        Title = "Database Schema Targets",
        MimeType = "text/markdown")]
    [Description("List configured database schema target aliases. No connection strings or credentials are exposed.")]
    public async Task<TextResourceContents> GetDatabaseTargetsResource(CancellationToken cancellationToken = default)
    {
        var targets = await _provider.ListTargetsAsync(cancellationToken);

        return new TextResourceContents
        {
            Uri = "database-schema://targets",
            MimeType = "text/markdown",
            Text = BuildTargetsMarkdown(targets)
        };
    }

    [McpServerResource(
        UriTemplate = "database-schema://target/{target}/databases",
        Name = "database_schema_target_databases",
        Title = "Database Target Databases",
        MimeType = "text/markdown")]
    [Description("List non-system databases visible on one configured database schema target.")]
    public async Task<TextResourceContents> GetTargetDatabasesResource(string target, CancellationToken cancellationToken = default)
    {
        var normalizedTarget = NormalizeRequired(target, nameof(target));
        var databases = await _provider.ListDatabasesAsync(
            normalizedTarget,
            includeSystemDatabases: false,
            cancellationToken);

        return new TextResourceContents
        {
            Uri = TargetDatabasesResourceUri(normalizedTarget),
            MimeType = "text/markdown",
            Text = BuildDatabasesMarkdown(normalizedTarget, databases)
        };
    }

    [McpServerResource(
        UriTemplate = "database-schema://target/{target}/database/{database}/tables",
        Name = "database_schema_target_database_tables",
        Title = "Database Target Tables",
        MimeType = "text/markdown")]
    [Description("List tables from one database/schema on one configured database schema target.")]
    public async Task<TextResourceContents> GetTargetDatabaseTablesResource(
        string target,
        string database,
        CancellationToken cancellationToken = default)
    {
        var normalizedTarget = NormalizeRequired(target, nameof(target));
        var normalizedDatabase = NormalizeRequired(database, nameof(database));
        var result = await _provider.SearchAsync(TargetDatabaseQuery(normalizedTarget, normalizedDatabase, includeColumns: false), cancellationToken);

        return new TextResourceContents
        {
            Uri = DatabaseTablesResourceUri(normalizedTarget, normalizedDatabase),
            MimeType = "text/markdown",
            Text = BuildSchemaMarkdown(result, title: $"Database tables: {normalizedTarget}/{normalizedDatabase}")
        };
    }

    [McpServerResource(
        UriTemplate = "database-schema://target/{target}/database/{database}/table/{table}",
        Name = "database_schema_target_database_table",
        Title = "Database Target Table Schema",
        MimeType = "text/markdown")]
    [Description("Get one table schema from one database/schema on one configured database schema target.")]
    public async Task<TextResourceContents> GetTargetDatabaseTableResource(
        string target,
        string database,
        string table,
        CancellationToken cancellationToken = default)
    {
        var normalizedTarget = NormalizeRequired(target, nameof(target));
        var normalizedDatabase = NormalizeRequired(database, nameof(database));
        var normalizedTable = NormalizeRequired(table, nameof(table));
        var query = TargetDatabaseQuery(normalizedTarget, normalizedDatabase, includeColumns: true) with
        {
            Table = normalizedTable,
            Limit = 1
        };
        var result = await _provider.SearchAsync(query, cancellationToken);
        var tableResult = FindFirstTable(result);
        var text = tableResult is null
            ? $"# Database table schema: {EscapeMarkdown(normalizedTable)}{Environment.NewLine}{Environment.NewLine}No schema found for this table."
            : BuildTableMarkdown(tableResult.Value.Target, tableResult.Value.Table);

        return new TextResourceContents
        {
            Uri = DatabaseTableResourceUri(normalizedTarget, normalizedDatabase, normalizedTable),
            MimeType = "text/markdown",
            Text = text
        };
    }

    private static DatabaseSchemaQuery DefaultResourceQuery(bool includeColumns) => new()
    {
        Limit = DefaultLimit,
        MaxDatabases = DefaultMaxDatabases,
        IncludeColumns = includeColumns
    };

    private static DatabaseSchemaQuery TargetDatabaseQuery(string target, string database, bool includeColumns) => new()
    {
        TargetAliases = [target],
        Database = database,
        Limit = DefaultLimit,
        MaxDatabases = 1,
        IncludeColumns = includeColumns
    };

    private static object Source(DatabaseSchemaSearchResult result) => new
    {
        system = SourceSystem,
        metadata_source = MetadataSource,
        target_count = result.Targets.Count,
        database_count = result.DatabaseCount
    };

    private static object ToTargetResult(DatabaseSchemaTargetResult target, bool includeColumns) => new
    {
        target = target.Target,
        display_name = target.DisplayName,
        current_database = target.CurrentDatabase,
        databases = target.Databases.Select(database => ToDatabaseResult(target.Target, database, includeColumns)).ToArray()
    };

    private static object ToDatabaseResult(string target, DatabaseSchemaDatabaseResult database, bool includeColumns) => new
    {
        database = database.Database,
        count = database.Tables.Count,
        resource_uri = DatabaseTablesResourceUri(target, database.Database),
        tables = database.Tables.Select(table => ToTableResult(target, table, includeColumns)).ToArray()
    };

    private static object ToTableResult(string target, DatabaseTableSchema table, bool includeColumns) => new
    {
        table_schema = table.TableSchema,
        table_name = table.TableName,
        table_type = table.TableType,
        engine = table.Engine,
        table_rows = table.TableRows,
        create_time = table.CreateTime,
        update_time = table.UpdateTime,
        table_comment = table.TableComment,
        resource_uri = DatabaseTableResourceUri(target, table.TableSchema, table.TableName),
        columns = includeColumns
            ? table.Columns.Select(static column => new
            {
                table_schema = column.TableSchema,
                ordinal_position = column.OrdinalPosition,
                column_name = column.ColumnName,
                data_type = column.DataType,
                column_type = column.ColumnType,
                is_nullable = column.IsNullable,
                column_key = column.ColumnKey,
                column_default = column.ColumnDefault,
                extra = column.Extra,
                column_comment = column.ColumnComment
            }).ToArray()
            : null
    };

    private static string BuildTargetsMarkdown(IReadOnlyList<DatabaseSchemaTargetInfo> targets)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Database schema targets");
        builder.AppendLine();
        builder.AppendLine("Configured target aliases only. Connection strings and credentials are not exposed.");
        builder.AppendLine();
        builder.AppendLine("| Target | Display name | Default | Resource |");
        builder.AppendLine("|---|---|---|---|");

        foreach (var target in targets)
        {
            builder
                .Append("| ")
                .Append(EscapeMarkdown(target.Alias))
                .Append(" | ")
                .Append(EscapeMarkdown(target.DisplayName))
                .Append(" | ")
                .Append(target.IsDefault ? "yes" : "no")
                .Append(" | `")
                .Append(TargetDatabasesResourceUri(target.Alias))
                .AppendLine("` |");
        }

        return builder.ToString();
    }

    private static string BuildDatabasesMarkdown(string target, IReadOnlyList<string> databases)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Databases for target: {EscapeMarkdown(target)}");
        builder.AppendLine();
        builder.AppendLine("Source: MySQL `information_schema.schemata`.");
        builder.AppendLine("System schemas are excluded from this resource.");
        builder.AppendLine();
        builder.AppendLine("| Database | Tables resource |");
        builder.AppendLine("|---|---|");

        foreach (var database in databases)
        {
            builder
                .Append("| ")
                .Append(EscapeMarkdown(database))
                .Append(" | `")
                .Append(DatabaseTablesResourceUri(target, database))
                .AppendLine("` |");
        }

        return builder.ToString();
    }

    private static string BuildSchemaMarkdown(DatabaseSchemaSearchResult result, string title)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {EscapeMarkdown(title)}");
        builder.AppendLine();
        builder.AppendLine("Source: MySQL `information_schema` via configured database schema target aliases.");
        builder.AppendLine("This resource exposes schema metadata only; it does not expose business rows.");

        foreach (var target in result.Targets)
        {
            builder.AppendLine();
            builder.AppendLine($"## Target: {EscapeMarkdown(target.Target)}");
            builder.AppendLine();
            builder.AppendLine($"- Display name: {EscapeMarkdown(target.DisplayName)}");
            builder.AppendLine($"- Current database: `{EscapeMarkdown(target.CurrentDatabase)}`");

            foreach (var database in target.Databases)
            {
                builder.AppendLine();
                builder.AppendLine($"### Database: `{EscapeMarkdown(database.Database)}`");
                builder.AppendLine();
                builder.AppendLine("| Table | Type | Engine | Approx. rows | Resource | Comment |");
                builder.AppendLine("|---|---|---|---:|---|---|");

                foreach (var table in database.Tables)
                {
                    builder
                        .Append("| ")
                        .Append(EscapeMarkdown(table.TableName))
                        .Append(" | ")
                        .Append(EscapeMarkdown(table.TableType))
                        .Append(" | ")
                        .Append(EscapeMarkdown(table.Engine))
                        .Append(" | ")
                        .Append(table.TableRows?.ToString() ?? string.Empty)
                        .Append(" | `")
                        .Append(DatabaseTableResourceUri(target.Target, database.Database, table.TableName))
                        .Append("` | ")
                        .Append(EscapeMarkdown(table.TableComment))
                        .AppendLine(" |");
                }
            }
        }

        return builder.ToString();
    }

    private static string BuildTableMarkdown(DatabaseSchemaTargetResult target, DatabaseTableSchema table)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Database table schema: {EscapeMarkdown(table.TableName)}");
        builder.AppendLine();
        builder.AppendLine($"- Target: `{EscapeMarkdown(target.Target)}`");
        builder.AppendLine($"- Target display name: {EscapeMarkdown(target.DisplayName)}");
        builder.AppendLine($"- Database: `{EscapeMarkdown(table.TableSchema)}`");
        builder.AppendLine($"- Type: {EscapeMarkdown(table.TableType)}");
        builder.AppendLine($"- Engine: {EscapeMarkdown(table.Engine)}");
        builder.AppendLine($"- Approx. rows: {table.TableRows?.ToString() ?? string.Empty}");
        builder.AppendLine($"- Comment: {EscapeMarkdown(table.TableComment)}");
        builder.AppendLine();
        builder.AppendLine("| # | Column | Type | Nullable | Key | Default | Extra | Comment |");
        builder.AppendLine("|---:|---|---|---|---|---|---|---|");

        foreach (var column in table.Columns)
        {
            builder
                .Append("| ")
                .Append(column.OrdinalPosition)
                .Append(" | ")
                .Append(EscapeMarkdown(column.ColumnName))
                .Append(" | ")
                .Append(EscapeMarkdown(column.ColumnType))
                .Append(" | ")
                .Append(EscapeMarkdown(column.IsNullable))
                .Append(" | ")
                .Append(EscapeMarkdown(column.ColumnKey))
                .Append(" | ")
                .Append(EscapeMarkdown(column.ColumnDefault))
                .Append(" | ")
                .Append(EscapeMarkdown(column.Extra))
                .Append(" | ")
                .Append(EscapeMarkdown(column.ColumnComment))
                .AppendLine(" |");
        }

        return builder.ToString();
    }

    private static int ValidateLimit(int limit)
    {
        if (limit is < 1 or > MaxLimit)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), $"Limit must be between 1 and {MaxLimit}.");
        }

        return limit;
    }

    private static int ValidateMaxDatabases(int maxDatabases)
    {
        if (maxDatabases is < 1 or > MaxDatabases)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDatabases), $"Max databases must be between 1 and {MaxDatabases}.");
        }

        return maxDatabases;
    }

    private static (bool AllTargets, IReadOnlyList<string>? TargetAliases) ParseTargetSelection(string? target, string? targets)
    {
        var normalizedTarget = NormalizeOptional(target);
        var normalizedTargets = NormalizeOptional(targets);

        if (normalizedTarget is not null && normalizedTargets is not null)
        {
            throw new ArgumentException("Specify either target or targets, not both.", nameof(targets));
        }

        if (normalizedTarget is not null)
        {
            return (false, [normalizedTarget]);
        }

        if (normalizedTargets is null)
        {
            return (false, null);
        }

        if (normalizedTargets.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return (true, null);
        }

        var targetAliases = normalizedTargets
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static alias => alias.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (targetAliases.Length == 0)
        {
            throw new ArgumentException("At least one target alias is required.", nameof(targets));
        }

        return (false, targetAliases);
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        var normalized = NormalizeOptional(value);

        return normalized ?? throw new ArgumentException($"{parameterName} is required.", parameterName);
    }

    private static (DatabaseSchemaTargetResult Target, DatabaseTableSchema Table)? FindFirstTable(DatabaseSchemaSearchResult result)
    {
        foreach (var target in result.Targets)
        {
            foreach (var database in target.Databases)
            {
                var table = database.Tables.FirstOrDefault();

                if (table is not null)
                {
                    return (target, table);
                }
            }
        }

        return null;
    }

    private static string TableResourceUri(string table) => $"database-schema://table/{Uri.EscapeDataString(table)}";

    private static string TargetDatabasesResourceUri(string target) =>
        $"database-schema://target/{Uri.EscapeDataString(target)}/databases";

    private static string DatabaseTablesResourceUri(string target, string database) =>
        $"database-schema://target/{Uri.EscapeDataString(target)}/database/{Uri.EscapeDataString(database)}/tables";

    private static string DatabaseTableResourceUri(string target, string database, string table) =>
        $"database-schema://target/{Uri.EscapeDataString(target)}/database/{Uri.EscapeDataString(database)}/table/{Uri.EscapeDataString(table)}";

    private static string EscapeMarkdown(string? value) =>
        string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}
