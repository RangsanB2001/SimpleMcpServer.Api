using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class DailyStatTools
{
    private readonly IDailyStatProvider _provider;

    public DailyStatTools(IDailyStatProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "daily_stats")]
    [Description(
        "Get end-of-day statistics for a Thai SET/mai stock from the d_stat table. " +
        "Returns daily P/E, P/BV, dividend yield, market cap, listed share, EPS, book value, " +
        "12-month dividend yield, PEG ratio, dividend payout ratio, plus trading flags and benefits: " +
        "status (SP/H/NR/NP/CM), benefit (XD/XR/XE/XM/XB/XN/XW), npg_flag, notice_pending_receive_flag (NP/NR), " +
        "non_compliance_flag (NC), caution_flag (C/CB/CS/CG/CF), stabilization_flag (ST), call_market_flag (CM). " +
        "Returns the latest day if no date range is given; otherwise returns daily rows between fromDate and toDate inclusive.")]
    public async Task<object> GetDailyStats(
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
                throw new InvalidOperationException($"No daily stats found for symbol '{normalized}'");
            }

            return Decorate(latest);
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
            rows = rows.Select(Decorate).ToArray()
        };
    }

    private static object Decorate(DailyStat row) => new
    {
        row,
        decoded = new
        {
            marketType = PsimsLabels.MarketType(row.MkType),
            securityType = PsimsLabels.SecType(row.SecType),
            status = PsimsLabels.Sign(row.Status),
            benefit = PsimsLabels.Benefit(row.Benefit),
            cautionFlag = PsimsLabels.Sign(row.CautionFlag),
            noticePendingReceiveFlag = PsimsLabels.Sign(row.NoticePendingReceiveFlag),
            nonComplianceFlag = PsimsLabels.Sign(row.NonComplianceFlag),
            stabilizationFlag = PsimsLabels.Sign(row.StabilizationFlag),
            callMarketFlag = PsimsLabels.Sign(row.CallMarketFlag),
            isNonPerformingGroup = string.Equals(row.NpgFlag, "Y", StringComparison.OrdinalIgnoreCase)
        }
    };
}
