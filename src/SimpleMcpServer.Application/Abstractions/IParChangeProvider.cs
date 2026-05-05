using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IParChangeProvider
{
    Task<IEnumerable<ParChange>> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}
