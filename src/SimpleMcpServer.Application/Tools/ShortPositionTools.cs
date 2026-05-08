using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class ShortPositionTools
{
    private readonly IShortPositionProvider _provider;

    public ShortPositionTools(IShortPositionProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "short_position")]
    [Description(
        "Get short-sale outstanding (shares sold short and not yet bought back) for a Thai SET/mai stock " +
        "from the shortpos table. Available since PSIMS#5 (2024-05). " +
        "Returns the latest day if no date range is given.")]
    public async Task<object> GetShortPosition(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("Optional start date in yyyy-MM-dd. Required together with toDate to fetch a range.")] string? fromDate = null,
        [Description("Optional end date in yyyy-MM-dd. Required together with fromDate to fetch a range.")] string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = SymbolHelpers.Normalize(symbol);

        if (fromDate is null && toDate is null)
        {
            var latest = await _provider.GetLatestAsync(normalized, cancellationToken);

            if (latest is null)
            {
                throw new InvalidOperationException($"No short position data found for symbol '{normalized}'");
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

        var rows = (await _provider.GetByDateRangeAsync(normalized, from, to, cancellationToken)).ToArray();

        return new
        {
            symbol = normalized,
            from = from.ToString("yyyy-MM-dd"),
            to = to.ToString("yyyy-MM-dd"),
            count = rows.Length,
            rows
        };
    }
}
