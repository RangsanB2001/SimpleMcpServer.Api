using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
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
    private const string SourceSystem = "PSIMS";
    private const string MetadataSource = "MySQL information_schema";
    private readonly IDatabaseSchemaProvider _provider;

    public DatabaseSchemaTools(IDatabaseSchemaProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "database_schema")]
    [Description(
        "Get read-only database schema metadata for the configured PSIMS MySQL database. " +
        "Use this to understand available tables and columns before choosing the right financial data tool. " +
        "Returns table metadata from information_schema and, optionally, column definitions. No business rows are returned.")]
    public async Task<object> GetDatabaseSchema(
        [Description("Exact table name to inspect, e.g. Compsec, Company, d_trade. Optional.")] string? table = null,
        [Description("Keyword to match table name or table comment. Ignored when table is provided. Optional.")] string? keyword = null,
        [Description("Maximum tables to return, 1-200. Default 50.")] int limit = DefaultLimit,
        [Description("Include column definitions for matched tables. Default true.")] bool include_columns = true,
        CancellationToken cancellationToken = default)
    {
        var boundedLimit = ValidateLimit(limit);
        var normalizedTable = NormalizeOptional(table);
        var normalizedKeyword = NormalizeOptional(keyword);

        if (normalizedTable is not null)
        {
            var exactTable = await _provider.GetTableAsync(normalizedTable, cancellationToken);

            if (exactTable is null)
            {
                throw new InvalidOperationException($"No database schema found for table '{normalizedTable}'.");
            }

            return new
            {
                tool = "database_schema",
                query = new
                {
                    table = normalizedTable,
                    include_columns
                },
                source = Source(exactTable.TableSchema),
                resource_uri = TableResourceUri(exactTable.TableName),
                table = ToTableResult(exactTable, include_columns)
            };
        }

        var tables = await _provider.SearchTablesAsync(
            table: null,
            keyword: normalizedKeyword,
            limit: boundedLimit,
            includeColumns: include_columns,
            cancellationToken);

        return new
        {
            tool = "database_schema",
            query = new
            {
                keyword = normalizedKeyword,
                limit = boundedLimit,
                include_columns
            },
            source = Source(tables.FirstOrDefault()?.TableSchema),
            count = tables.Count,
            resource_uri = "database-schema://tables",
            tables = tables.Select(tableSchema => ToTableResult(tableSchema, include_columns)).ToArray()
        };
    }

    [McpServerResource(
        UriTemplate = "database-schema://tables",
        Name = "database_schema_tables",
        Title = "PSIMS Database Tables",
        MimeType = "text/markdown")]
    [Description("List PSIMS database tables from MySQL information_schema for AI context.")]
    public async Task<TextResourceContents> GetDatabaseTablesResource(CancellationToken cancellationToken = default)
    {
        var tables = await _provider.SearchTablesAsync(
            table: null,
            keyword: null,
            limit: DefaultLimit,
            includeColumns: false,
            cancellationToken);

        return new TextResourceContents
        {
            Uri = "database-schema://tables",
            MimeType = "text/markdown",
            Text = BuildTablesMarkdown(tables)
        };
    }

    [McpServerResource(
        UriTemplate = "database-schema://table/{table}",
        Name = "database_schema_table",
        Title = "PSIMS Database Table Schema",
        MimeType = "text/markdown")]
    [Description("Get one PSIMS database table schema, including column definitions, from MySQL information_schema.")]
    public async Task<TextResourceContents> GetDatabaseTableResource(string table, CancellationToken cancellationToken = default)
    {
        var normalizedTable = NormalizeRequiredTable(table);
        var tableSchema = await _provider.GetTableAsync(normalizedTable, cancellationToken);
        var text = tableSchema is null
            ? $"# Database table schema: {EscapeMarkdown(normalizedTable)}{Environment.NewLine}{Environment.NewLine}No schema found for this table."
            : BuildTableMarkdown(tableSchema);

        return new TextResourceContents
        {
            Uri = TableResourceUri(normalizedTable),
            MimeType = "text/markdown",
            Text = text
        };
    }

    private static object Source(string? database) => new
    {
        system = SourceSystem,
        database,
        metadata_source = MetadataSource
    };

    private static object ToTableResult(DatabaseTableSchema table, bool includeColumns) => new
    {
        table_schema = table.TableSchema,
        table_name = table.TableName,
        table_type = table.TableType,
        engine = table.Engine,
        table_rows = table.TableRows,
        create_time = table.CreateTime,
        update_time = table.UpdateTime,
        table_comment = table.TableComment,
        resource_uri = TableResourceUri(table.TableName),
        columns = includeColumns
            ? table.Columns.Select(static column => new
            {
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

    private static string BuildTablesMarkdown(IReadOnlyList<DatabaseTableSchema> tables)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# PSIMS database tables");
        builder.AppendLine();
        builder.AppendLine("Source: MySQL `information_schema` via configured `DefaultConnection`.");
        builder.AppendLine("This resource exposes schema metadata only; it does not expose business rows.");
        builder.AppendLine();
        builder.AppendLine("| Table | Type | Engine | Approx. rows | Comment |");
        builder.AppendLine("|---|---|---|---:|---|");

        foreach (var table in tables)
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
                .Append(" | ")
                .Append(EscapeMarkdown(table.TableComment))
                .AppendLine(" |");
        }

        return builder.ToString();
    }

    private static string BuildTableMarkdown(DatabaseTableSchema table)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Database table schema: {EscapeMarkdown(table.TableName)}");
        builder.AppendLine();
        builder.AppendLine($"- Schema: `{EscapeMarkdown(table.TableSchema)}`");
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

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string NormalizeRequiredTable(string table)
    {
        var normalized = NormalizeOptional(table);

        return normalized ?? throw new ArgumentException("Table name is required.", nameof(table));
    }

    private static string TableResourceUri(string table) => $"database-schema://table/{Uri.EscapeDataString(table)}";

    private static string EscapeMarkdown(string? value) =>
        string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}
