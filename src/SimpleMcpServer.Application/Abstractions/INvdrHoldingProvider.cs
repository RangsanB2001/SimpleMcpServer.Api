using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface INvdrHoldingProvider
{
    Task<NvdrHolding?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<NvdrHolding>> GetByDateRangeAsync(
        string symbol,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
