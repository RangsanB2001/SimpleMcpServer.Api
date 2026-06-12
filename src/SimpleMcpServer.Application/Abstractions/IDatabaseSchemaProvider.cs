using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IDatabaseSchemaProvider
{
    Task<IReadOnlyList<DatabaseTableSchema>> SearchTablesAsync(
        string? table,
        string? keyword,
        int limit,
        bool includeColumns,
        CancellationToken cancellationToken = default);

    Task<DatabaseTableSchema?> GetTableAsync(string table, CancellationToken cancellationToken = default);
}
