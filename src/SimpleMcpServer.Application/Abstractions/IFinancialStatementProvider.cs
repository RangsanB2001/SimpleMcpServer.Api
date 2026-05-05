using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IFinancialStatementProvider
{
    Task<IEnumerable<FinancialStatementLine>> GetByPeriodAsync(
        string symbol,
        int fiscal,
        string quarter,
        string finStateType,
        CancellationToken cancellationToken = default);
}
