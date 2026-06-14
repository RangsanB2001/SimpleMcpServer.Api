using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IInvestorBreakdownProvider
{
    Task<InvestorBreakdown?> GetLatestAsync(string marketType, CancellationToken cancellationToken = default);

    Task<IEnumerable<InvestorBreakdown>> GetByDateRangeAsync(
        string marketType,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
