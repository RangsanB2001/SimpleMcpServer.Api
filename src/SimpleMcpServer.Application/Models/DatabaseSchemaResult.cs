using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Models;

public sealed class DatabaseSchemaSearchResult
{
    public IReadOnlyList<DatabaseSchemaTargetResult> Targets { get; init; } = [];

    public int DatabaseCount => Targets.Sum(static target => target.Databases.Count);

    public int TableCount => Targets.Sum(static target => target.Databases.Sum(static database => database.Tables.Count));
}

public sealed class DatabaseSchemaTargetResult
{
    public string Target { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? CurrentDatabase { get; init; }

    public IReadOnlyList<DatabaseSchemaDatabaseResult> Databases { get; init; } = [];
}

public sealed class DatabaseSchemaDatabaseResult
{
    public string Database { get; init; } = string.Empty;

    public IReadOnlyList<DatabaseTableSchema> Tables { get; init; } = [];
}

public sealed class DatabaseSchemaTargetInfo
{
    public string Alias { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public bool IsDefault { get; init; }
}
