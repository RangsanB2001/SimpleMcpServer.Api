using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class FundamentalLookupTool
{
    private readonly IFundamentalProvider _provider;

    public FundamentalLookupTool(IFundamentalProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "fundamental_lookup")]
    [Description(
        "Get Thai SET/mai stock fundamental data (P/E, P/BV, ROE, ROA, EPS, market cap, balance sheet) " +
        "for a given symbol. Returns the latest quarter by default; pass fiscal and quarter to fetch a specific period.")]
    public async Task<FundamentalData> Execute(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK, AOT.")] string symbol,
        [Description("Optional fiscal year (e.g. 2024). Omit to get the latest available quarter.")] int? fiscal = null,
        [Description("Optional quarter: '1', '2', '3' for quarterly, '9' for annual. Omit to get the latest available quarter.")] string? quarter = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("symbol must not be empty", nameof(symbol));
        }

        if (quarter is not null && quarter is not ("1" or "2" or "3" or "9"))
        {
            throw new ArgumentException("quarter must be one of '1', '2', '3', '9'", nameof(quarter));
        }

        var result = await _provider.GetByPeriodAsync(symbol, fiscal, quarter, cancellationToken);

        if (result is null)
        {
            var period = (fiscal, quarter) switch
            {
                (not null, not null) => $" (fiscal {fiscal}, quarter {quarter})",
                (not null, null) => $" (fiscal {fiscal})",
                (null, not null) => $" (quarter {quarter})",
                _ => string.Empty
            };

            throw new InvalidOperationException(
                $"No fundamental data found for symbol '{symbol}'{period}");
        }

        return result;
    }
}
