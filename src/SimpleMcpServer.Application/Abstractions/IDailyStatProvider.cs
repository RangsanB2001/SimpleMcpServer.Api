using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IDailyStatProvider
{
    Task<DailyStat?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<DailyStat>> GetByDateRangeAsync(
        string symbol,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
