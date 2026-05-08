using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface IForeignRoomProvider
{
    Task<ForeignRoom?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<ForeignRoom>> GetByDateRangeAsync(
        string symbol,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
