using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class InvestorBreakdownTools
{
    private readonly IInvestorBreakdownProvider _provider;

    public InvestorBreakdownTools(IInvestorBreakdownProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "investor_breakdown")]
    [Description(
        "Get daily market-wide investor-type buy/sell breakdown from the D_Cust table. " +
        "Categories: Customer (retail), Institute (mutual fund / local institute), Foreigner, Broker (proprietary). " +
        "Each category includes transactions, volume, and value for both buy and sell. " +
        "Pass marketType='A' for SET (default) or 'S' for mai. " +
        "Returns the latest day if no date range is given.")]
    public async Task<object> GetInvestorBreakdown(
        [Description("Market type: 'A' = SET (default), 'S' = mai.")] string marketType = "A",
        [Description("Optional start date in yyyy-MM-dd. Required together with toDate to fetch a range.")] string? fromDate = null,
        [Description("Optional end date in yyyy-MM-dd. Required together with fromDate to fetch a range.")] string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (marketType is not ("A" or "S"))
        {
            throw new ArgumentException("marketType must be 'A' or 'S'", nameof(marketType));
        }

        if (fromDate is null && toDate is null)
        {
            var latest = await _provider.GetLatestAsync(marketType, cancellationToken);

            if (latest is null)
            {
                throw new InvalidOperationException($"No investor breakdown data found for market '{marketType}'");
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

        var rows = (await _provider.GetByDateRangeAsync(marketType, from, to, cancellationToken)).ToArray();

        return new
        {
            marketType,
            from = from.ToString("yyyy-MM-dd"),
            to = to.ToString("yyyy-MM-dd"),
            count = rows.Length,
            rows
        };
    }
}
