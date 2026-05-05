using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class DividendTools
{
    private const int DefaultLimit = 10;
    private const int MaxLimit = 100;

    private readonly IDividendProvider _provider;

    public DividendTools(IDividendProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "dividends")]
    [Description(
        "Get dividend announcements for a Thai SET/mai stock from the Dividend table. " +
        "Returns the most recent N events (cancelled rows excluded). " +
        "Each row includes XD/payment dates, dividend type (CD=cash, SD=stock), amount per share, and source of payment.")]
    public async Task<object> GetDividends(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("Maximum rows to return (1-100, default 10).")] int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("symbol must not be empty", nameof(symbol));
        }

        if (limit < 1 || limit > MaxLimit)
        {
            throw new ArgumentException($"limit must be between 1 and {MaxLimit}", nameof(limit));
        }

        var rows = (await _provider.GetBySymbolAsync(symbol, limit, cancellationToken)).ToArray();

        return new
        {
            symbol,
            count = rows.Length,
            rows
        };
    }
}
