namespace SimpleMcpServer.Application.Models;

public sealed record DatabaseSchemaQuery
{
    public IReadOnlyList<string>? TargetAliases { get; init; }

    public bool AllTargets { get; init; }

    public string? Database { get; init; }

    public bool AllDatabases { get; init; }

    public bool IncludeSystemDatabases { get; init; }

    public string? Table { get; init; }

    public string? Keyword { get; init; }

    public int Limit { get; init; }

    public int MaxDatabases { get; init; }

    public bool IncludeColumns { get; init; }
}
