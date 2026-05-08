using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IFreeFloatProvider
{
    Task<FreeFloat?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<FreeFloat>> GetHistoryAsync(string symbol, int limit, CancellationToken cancellationToken = default);
}
