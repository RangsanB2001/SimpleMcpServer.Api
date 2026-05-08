using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class NvdrHoldingTools
{
    private readonly INvdrHoldingProvider _provider;

    public NvdrHoldingTools(INvdrHoldingProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "nvdr_holdings")]
    [Description(
        "Get NVDR (Non-Voting Depositary Receipt) holdings for a Thai SET/mai stock from the Nvdr table. " +
        "Returns shares held by Thai NVDR Co., Ltd., average cost, and percentage of trading volume/value. " +
        "Pass the underlying symbol (e.g. PTT) — the Nvdr table tracks the SEC_NAME-R notation internally. " +
        "Returns the latest day if no date range is given.")]
    public async Task<object> GetNvdrHoldings(
        [Description("SET/mai stock symbol (typically the -R suffixed name as stored in Nvdr table, e.g. PTT-R).")] string symbol,
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
                throw new InvalidOperationException($"No NVDR holdings found for symbol '{normalized}'");
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
