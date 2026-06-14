using System.Text.Json;
using System.Text.Json.Nodes;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.Models;
using SimpleMcpServer.Application.Tools;
using SimpleMcpServer.Domain.Entities;
using Xunit;

namespace SimpleMcpServer.Application.Tests;

public class DatabaseSchemaToolsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task DatabaseSchema_DefaultScopeUsesDefaultTargetAndCurrentDatabase()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            SearchResult = SampleResult()
        };
        var tool = new DatabaseSchemaTools(provider);

        var json = ToJson(await tool.GetDatabaseSchema(table: " Compsec "));
        var query = AssertLastQuery(provider);

        Assert.Equal("database_schema", json["tool"]!.GetValue<string>());
        Assert.Null(query.TargetAliases);
        Assert.False(query.AllTargets);
        Assert.Null(query.Database);
        Assert.False(query.AllDatabases);
        Assert.Equal("Compsec", query.Table);
        Assert.Equal("psims", json["targets"]![0]!["target"]!.GetValue<string>());
        Assert.Equal("psims", json["targets"]![0]!["databases"]![0]!["database"]!.GetValue<string>());
        Assert.Equal("sec_name", json["targets"]![0]!["databases"]![0]!["tables"]![0]!["columns"]![0]!["column_name"]!.GetValue<string>());
    }

    [Fact]
    public async Task DatabaseSchema_SingleTargetAndDatabaseForwardScope()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            SearchResult = SampleResult(target: "psims-uat", displayName: "PSIMS UAT", database: "uatdb")
        };
        var tool = new DatabaseSchemaTools(provider);

        var json = ToJson(await tool.GetDatabaseSchema(
            target: " psims-uat ",
            database: " uatdb ",
            keyword: "sec",
            limit: 10,
            include_columns: false));
        var query = AssertLastQuery(provider);

        Assert.Equal(["psims-uat"], query.TargetAliases);
        Assert.Equal("uatdb", query.Database);
        Assert.Equal("sec", query.Keyword);
        Assert.Equal(10, query.Limit);
        Assert.False(query.IncludeColumns);
        Assert.Equal(1, json["target_count"]!.GetValue<int>());
        Assert.True(json["targets"]![0]!["databases"]![0]!["tables"]![0]!["columns"] is null);
    }

    [Fact]
    public async Task DatabaseSchema_CommaSeparatedTargetsForwardAliases()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            SearchResult = SampleResult()
        };
        var tool = new DatabaseSchemaTools(provider);

        _ = await tool.GetDatabaseSchema(targets: "psims, psims-uat");
        var query = AssertLastQuery(provider);

        Assert.False(query.AllTargets);
        Assert.Equal(["psims", "psims-uat"], query.TargetAliases);
    }

    [Fact]
    public async Task DatabaseSchema_AllTargetsForwardAllTargetFlag()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            SearchResult = SampleResult()
        };
        var tool = new DatabaseSchemaTools(provider);

        _ = await tool.GetDatabaseSchema(targets: "all", all_databases: true, max_databases: 5);
        var query = AssertLastQuery(provider);

        Assert.True(query.AllTargets);
        Assert.Null(query.TargetAliases);
        Assert.True(query.AllDatabases);
        Assert.Equal(5, query.MaxDatabases);
    }

    [Fact]
    public async Task DatabaseSchema_RejectsAmbiguousTargetSelection()
    {
        var tool = new DatabaseSchemaTools(new FakeDatabaseSchemaProvider());

        await Assert.ThrowsAsync<ArgumentException>(() => tool.GetDatabaseSchema(target: "psims", targets: "all"));
    }

    [Fact]
    public async Task DatabaseSchema_RejectsDatabaseWithAllDatabases()
    {
        var tool = new DatabaseSchemaTools(new FakeDatabaseSchemaProvider());

        await Assert.ThrowsAsync<ArgumentException>(() => tool.GetDatabaseSchema(database: "psims", all_databases: true));
    }

    [Fact]
    public async Task DatabaseSchema_RejectsOutOfRangeLimit()
    {
        var tool = new DatabaseSchemaTools(new FakeDatabaseSchemaProvider());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => tool.GetDatabaseSchema(limit: 0));
    }

    [Fact]
    public async Task DatabaseSchema_RejectsOutOfRangeMaxDatabases()
    {
        var tool = new DatabaseSchemaTools(new FakeDatabaseSchemaProvider());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => tool.GetDatabaseSchema(max_databases: 0));
    }

    [Fact]
    public async Task DatabaseSchema_WithExactTableThrowsWhenNoTableFound()
    {
        var tool = new DatabaseSchemaTools(new FakeDatabaseSchemaProvider
        {
            SearchResult = new DatabaseSchemaSearchResult()
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => tool.GetDatabaseSchema(table: "MissingTable"));
    }

    [Fact]
    public async Task DatabaseTargetsResource_ReturnsConfiguredAliases()
    {
        var provider = new FakeDatabaseSchemaProvider();
        var tool = new DatabaseSchemaTools(provider);

        var resource = await tool.GetDatabaseTargetsResource();

        Assert.Equal("database-schema://targets", resource.Uri);
        Assert.Equal("text/markdown", resource.MimeType);
        Assert.Contains("| psims | PSIMS default | yes |", resource.Text);
        Assert.Contains("database-schema://target/psims/databases", resource.Text);
    }

    [Fact]
    public async Task TargetDatabasesResource_ForwardsTargetAndReturnsMarkdown()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            Databases = ["psims", "market"]
        };
        var tool = new DatabaseSchemaTools(provider);

        var resource = await tool.GetTargetDatabasesResource(" psims ");

        Assert.Equal("psims", provider.LastDatabaseListTarget);
        Assert.False(provider.LastDatabaseListIncludeSystemSchemas);
        Assert.Equal("database-schema://target/psims/databases", resource.Uri);
        Assert.Contains("database-schema://target/psims/database/market/tables", resource.Text);
    }

    [Fact]
    public async Task TargetDatabaseTablesResource_UsesTargetAndDatabaseResourceUri()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            SearchResult = SampleResult(database: "market")
        };
        var tool = new DatabaseSchemaTools(provider);

        var resource = await tool.GetTargetDatabaseTablesResource("psims", "market");
        var query = AssertLastQuery(provider);

        Assert.Equal(["psims"], query.TargetAliases);
        Assert.Equal("market", query.Database);
        Assert.False(query.IncludeColumns);
        Assert.Equal("database-schema://target/psims/database/market/tables", resource.Uri);
        Assert.Contains("Database tables: psims/market", resource.Text);
    }

    [Fact]
    public async Task DatabaseTableResource_ReturnsMarkdownColumnContext()
    {
        var provider = new FakeDatabaseSchemaProvider
        {
            SearchResult = SampleResult()
        };
        var tool = new DatabaseSchemaTools(provider);

        var resource = await tool.GetDatabaseTableResource("Compsec");

        Assert.Equal("database-schema://table/Compsec", resource.Uri);
        Assert.Equal("text/markdown", resource.MimeType);
        Assert.Contains("# Database table schema: Compsec", resource.Text);
        Assert.Contains("- Target: `psims`", resource.Text);
        Assert.Contains("- Database: `psims`", resource.Text);
        Assert.Contains("| 1 | sec_name | varchar(20) | NO | PRI |", resource.Text);
    }

    private static DatabaseSchemaQuery AssertLastQuery(FakeDatabaseSchemaProvider provider)
    {
        Assert.NotNull(provider.LastQuery);

        return provider.LastQuery;
    }

    private static JsonObject ToJson(object value) =>
        JsonSerializer.SerializeToNode(value, JsonOptions)!.AsObject();

    private static DatabaseSchemaSearchResult SampleResult(
        string target = "psims",
        string displayName = "PSIMS default",
        string database = "psims") => new()
        {
            Targets =
            [
                new DatabaseSchemaTargetResult
                {
                    Target = target,
                    DisplayName = displayName,
                    CurrentDatabase = database,
                    Databases =
                    [
                        new DatabaseSchemaDatabaseResult
                        {
                            Database = database,
                            Tables = [SampleTable(database)]
                        }
                    ]
                }
            ]
        };

    private static DatabaseTableSchema SampleTable(string database) => new()
    {
        TableSchema = database,
        TableName = "Compsec",
        TableType = "BASE TABLE",
        Engine = "InnoDB",
        TableRows = 100,
        TableComment = "Security master",
        Columns =
        [
            new DatabaseColumnSchema
            {
                TableSchema = database,
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
        public DatabaseSchemaSearchResult SearchResult { get; init; } = SampleResult();

        public IReadOnlyList<DatabaseSchemaTargetInfo> Targets { get; init; } =
        [
            new DatabaseSchemaTargetInfo
            {
                Alias = "psims",
                DisplayName = "PSIMS default",
                IsDefault = true
            },
            new DatabaseSchemaTargetInfo
            {
                Alias = "psims-uat",
                DisplayName = "PSIMS UAT",
                IsDefault = false
            }
        ];

        public IReadOnlyList<string> Databases { get; init; } = ["psims"];

        public DatabaseSchemaQuery? LastQuery { get; private set; }

        public string? LastDatabaseListTarget { get; private set; }

        public bool LastDatabaseListIncludeSystemSchemas { get; private set; }

        public Task<DatabaseSchemaSearchResult> SearchAsync(
            DatabaseSchemaQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;

            return Task.FromResult(SearchResult);
        }

        public Task<IReadOnlyList<DatabaseSchemaTargetInfo>> ListTargetsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Targets);

        public Task<IReadOnlyList<string>> ListDatabasesAsync(
            string target,
            bool includeSystemDatabases,
            CancellationToken cancellationToken = default)
        {
            LastDatabaseListTarget = target;
            LastDatabaseListIncludeSystemSchemas = includeSystemDatabases;

            return Task.FromResult(Databases);
        }
    }
}
