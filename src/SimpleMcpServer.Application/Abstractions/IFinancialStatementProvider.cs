using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IFinancialStatementProvider
{
    Task<IEnumerable<FinancialStatementLine>> GetByPeriodAsync(
        string symbol,
        int fiscal,
        string quarter,
        string finStateType,
        string adjustFinState,
        CancellationToken cancellationToken = default);

    Task<FinancialStatementHeader?> GetHeaderByPeriodAsync(
        string symbol,
        int fiscal,
        string quarter,
        string finStateType,
        string adjustFinState,
        CancellationToken cancellationToken = default);
}
