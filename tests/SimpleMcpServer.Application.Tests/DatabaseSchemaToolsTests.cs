using System.Text.Json;
using System.Text.Json.Nodes;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.Tools;
using SimpleMcpServer.Domain.Entities;
using Xunit;

namespace SimpleMcpServer.Application.Tests;

public class DatabaseSchemaToolsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task DatabaseSchema_WithExactTableReturnsColumnsAndResourceUri()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            ExactTable = SampleTable()
        };
        var tool = new DatabaseSchemaTools(provider);

        var json = ToJson(await tool.GetDatabaseSchema(table: " Compsec "));

        Assert.Equal("database_schema", json["tool"]!.GetValue<string>());
        Assert.Equal("Compsec", provider.LastExactTable);
        Assert.Equal("database-schema://table/Compsec", json["resource_uri"]!.GetValue<string>());
        Assert.Equal("sec_name", json["table"]!["columns"]![0]!["column_name"]!.GetValue<string>());
    }

    [Fact]
    public async Task DatabaseSchema_SearchForwardsLimitAndColumnFlag()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            SearchTables = [SampleTable()]
        };
        var tool = new DatabaseSchemaTools(provider);

        var json = ToJson(await tool.GetDatabaseSchema(keyword: "sec", limit: 10, include_columns: false));

        Assert.Equal("sec", provider.LastKeyword);
        Assert.Equal(10, provider.LastLimit);
        Assert.False(provider.LastIncludeColumns);
        Assert.Equal(1, json["count"]!.GetValue<int>());
        Assert.True(json["tables"]![0]!["columns"] is null);
    }

    [Fact]
    public async Task DatabaseTableResource_ReturnsMarkdownColumnContext()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            ExactTable = SampleTable()
        };
        var tool = new DatabaseSchemaTools(provider);

        var resource = await tool.GetDatabaseTableResource("Compsec");

        Assert.Equal("database-schema://table/Compsec", resource.Uri);
        Assert.Equal("text/markdown", resource.MimeType);
        Assert.Contains("# Database table schema: Compsec", resource.Text);
        Assert.Contains("| 1 | sec_name | varchar(20) | NO | PRI |", resource.Text);
    }

    [Fact]
    public async Task DatabaseSchema_RejectsOutOfRangeLimit()
    {
        var tool = new DatabaseSchemaTools(new FakeDatabaseSchemaProvider());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => tool.GetDatabaseSchema(limit: 0));
    }

    private static JsonObject ToJson(object value) =>
        JsonSerializer.SerializeToNode(value, JsonOptions)!.AsObject();

    private static DatabaseTableSchema SampleTable() => new()
    {
        TableSchema = "psims",
        TableName = "Compsec",
        TableType = "BASE TABLE",
        Engine = "InnoDB",
        TableRows = 100,
        TableComment = "Security master",
        Columns =
        [
            new DatabaseColumnSchema
            {
                TableName = "Compsec",
                ColumnName = "sec_name",
                OrdinalPosition = 1,
                DataType = "varchar",
                ColumnType = "varchar(20)",
                IsNullable = "NO",
                ColumnKey = "PRI",
                ColumnDefault = null,
                Extra = string.Empty,
                ColumnComment = "Symbol"
            }
        ]
    };

    private sealed class FakeDatabaseSchemaProvider : IDatabaseSchemaProvider
    {
        public DatabaseTableSchema? ExactTable { get; init; }

        public IReadOnlyList<DatabaseTableSchema> SearchTables { get; init; } = [];

        public string? LastExactTable { get; private set; }

        public string? LastKeyword { get; private set; }

        public int LastLimit { get; private set; }

        public bool LastIncludeColumns { get; private set; }

        public Task<IReadOnlyList<DatabaseTableSchema>> SearchTablesAsync(
            string? table,
            string? keyword,
            int limit,
            bool includeColumns,
            CancellationToken cancellationToken = default)
        {
            LastKeyword = keyword;
            LastLimit = limit;
            LastIncludeColumns = includeColumns;

            return Task.FromResult(SearchTables);
        }

        public Task<DatabaseTableSchema?> GetTableAsync(string table, CancellationToken cancellationToken = default)
        {
            LastExactTable = table;

            return Task.FromResult(ExactTable);
        }
    }
}
