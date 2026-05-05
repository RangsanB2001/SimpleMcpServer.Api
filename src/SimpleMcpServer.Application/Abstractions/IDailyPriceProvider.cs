using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IDailyPriceProvider
{
    Task<DailyPrice?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<DailyPrice>> GetByDateRangeAsync(
        string symbol,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
