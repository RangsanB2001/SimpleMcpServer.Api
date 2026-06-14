using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface ISecurityMasterProvider
{
    Task<SecurityMaster?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
}
