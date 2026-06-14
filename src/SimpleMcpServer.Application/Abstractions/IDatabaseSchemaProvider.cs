using SimpleMcpServer.Application.Models;

namespace SimpleMcpServer.Application.Abstractions;

public interface IDatabaseSchemaProvider
{
    Task<DatabaseSchemaSearchResult> SearchAsync(
        DatabaseSchemaQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DatabaseSchemaTargetInfo>> ListTargetsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListDatabasesAsync(
        string target,
        bool includeSystemDatabases,
        CancellationToken cancellationToken = default);
}
