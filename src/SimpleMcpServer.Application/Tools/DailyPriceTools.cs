using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class DailyPriceTools
{
    private readonly IDailyPriceProvider _provider;

    public DailyPriceTools(IDailyPriceProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "daily_prices")]
    [Description(
        "Get end-of-day OHLCV (open/high/low/close/volume/value) for a Thai SET/mai stock from the d_trade table. " +
        "Returns the latest day if no date range is given; otherwise returns daily rows between fromDate and toDate inclusive. " +
        "Uses Auto-Matching trading method (the canonical exchange price).")]
    public async Task<object> GetDailyPrices([Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol, [Description("Optional start date in yyyy-MM-dd. Required together with toDate to fetch a range.")] string? fromDate = null, [Description("Optional end date in yyyy-MM-dd. Required together with fromDate to fetch a range.")] string? toDate = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("symbol must not be empty", nameof(symbol));
        }

        if (fromDate is null && toDate is null)
        {
            var latest = await _provider.GetLatestAsync(symbol, cancellationToken);

            if (latest is null)
            {
                throw new InvalidOperationException($"No daily price found for symbol '{symbol}'");
            }

            return latest;
        }

        if (fromDate is null || toDate is null)
        {
            throw new ArgumentException("fromDate and toDate must be provided together to fetch a range");
        }

        if (!DateTime.TryParse(fromDate, out var from) || !DateTime.TryParse(toDate, out var to))
        {
            throw new ArgumentException("fromDate and toDate must be valid dates in yyyy-MM-dd format");
        }

        if (to < from)
        {
            throw new ArgumentException("toDate must be on or after fromDate");
        }

        var rows = (await _provider.GetByDateRangeAsync(symbol, from, to, cancellationToken)).ToArray();

        return new
        {
            symbol,
            from = from.ToString("yyyy-MM-dd"),
            to = to.ToString("yyyy-MM-dd"),
            count = rows.Length,
            rows
        };
    }
}
