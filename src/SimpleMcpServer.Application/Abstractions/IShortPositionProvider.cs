using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IShortPositionProvider
{
    Task<ShortPosition?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<ShortPosition>> GetByDateRangeAsync(
        string symbol,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
