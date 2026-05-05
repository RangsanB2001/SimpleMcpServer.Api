using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IDividendProvider
{
    Task<IEnumerable<Dividend>> GetBySymbolAsync(
        string symbol,
        int limit,
        CancellationToken cancellationToken = default);
}
