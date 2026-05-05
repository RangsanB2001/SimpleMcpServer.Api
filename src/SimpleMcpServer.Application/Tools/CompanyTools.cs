using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class CompanyTools
{
    private readonly ICompanyProvider _provider;

    public CompanyTools(ICompanyProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "company_profile")]
    [Description(
        "Get company profile for a Thai SET/mai stock symbol. Joins the Company table with the business table " +
        "to return name (Thai/English), address, contact details, listing status, business description, " +
        "CG score, CAC anti-corruption flag, and dividend policy.")]
    public async Task<CompanyProfile> GetCompanyProfile(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("symbol must not be empty", nameof(symbol));
        }

        var profile = await _provider.GetBySymbolAsync(symbol, cancellationToken);

        if (profile is null)
        {
            throw new InvalidOperationException($"No company profile found for symbol '{symbol}'");
        }

        return profile;
    }
}
