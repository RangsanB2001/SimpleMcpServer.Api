using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Abstractions;

public interface ICompanyProvider
{
    Task<CompanyProfile?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}
