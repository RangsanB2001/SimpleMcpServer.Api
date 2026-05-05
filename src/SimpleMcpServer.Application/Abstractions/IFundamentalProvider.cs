using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IFundamentalProvider : IDataProvider<FundamentalData>
{
    Task<FundamentalData?> GetByPeriodAsync(
        string symbol,
        int? fiscal,
        string? quarter,
        CancellationToken cancellationToken = default);
}
