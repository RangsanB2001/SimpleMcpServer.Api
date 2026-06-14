using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface ITradingSignProvider
{
    Task<IEnumerable<TradingSign>> GetActiveAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<TradingSign>> GetHistoryAsync(string symbol, int limit, CancellationToken cancellationToken = default);

    Task<IEnumerable<TradingSignDetail>> GetReasonsAsync(string symbol, DateTime signPosDate, string sign, CancellationToken cancellationToken = default);
}
