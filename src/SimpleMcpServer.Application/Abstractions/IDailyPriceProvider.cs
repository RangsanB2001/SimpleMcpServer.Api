using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IDailyPriceProvider
{
    Task<DailyPrice?> GetLatestAsync(
        string symbol,
        string tradingMethod,
        string? subTypeOfTrade,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DailyPrice>> GetByDateRangeAsync(
        string symbol,
        DateTime fromDate,
        DateTime toDate,
        string tradingMethod,
        string? subTypeOfTrade,
        CancellationToken cancellationToken = default);
}
