using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Constants;
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
        "Defaults to Auto-Matching ('A', the canonical exchange price). " +
        "Pass tradingMethod='T' for Trade Report (Big Lot/Foreign-TR — often material on Thai stocks) or 'O' for Odd Lot. " +
        "When tradingMethod='T', subTypeOfTrade can narrow further: 'F'=Foreign, 'B'=Biglot, 'N'=Buy-in.")]
    public async Task<object> GetDailyPrices(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("Optional start date in yyyy-MM-dd. Required together with toDate to fetch a range.")] string? fromDate = null,
        [Description("Optional end date in yyyy-MM-dd. Required together with fromDate to fetch a range.")] string? toDate = null,
        [Description("Trading method: 'A'=Auto matching (default), 'T'=Trade Report, 'O'=Odd Lot.")] string tradingMethod = PsimsCodes.TradingMethod.AutoMatching,
        [Description("Sub-type of Trade Report (only when tradingMethod='T'): 'F'=Foreign, 'B'=Biglot, 'N'=Buy-in. Omit for all sub-types.")] string? subTypeOfTrade = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = SymbolHelpers.Normalize(symbol);

        if (tradingMethod is not (PsimsCodes.TradingMethod.AutoMatching or PsimsCodes.TradingMethod.TradeReport or PsimsCodes.TradingMethod.OddLot))
        {
            throw new ArgumentException("tradingMethod must be one of 'A', 'T', 'O'", nameof(tradingMethod));
        }

        if (subTypeOfTrade is not null && tradingMethod != PsimsCodes.TradingMethod.TradeReport)
        {
            throw new ArgumentException("subTypeOfTrade is only applicable when tradingMethod is 'T'", nameof(subTypeOfTrade));
        }

        if (fromDate is null && toDate is null)
        {
            var latest = await _provider.GetLatestAsync(normalized, tradingMethod, subTypeOfTrade, cancellationToken);

            if (latest is null)
            {
                throw new InvalidOperationException($"No daily price found for symbol '{normalized}'");
            }

            return new
            {
                symbol = normalized,
                row = latest,
                decoded = new
                {
                    marketType = PsimsLabels.MarketType(latest.MkType),
                    tradingMethod = PsimsLabels.TradingMethod(latest.TradingMethod),
                    subTypeOfTrade = PsimsLabels.TradeReportSubType(latest.SubTypeOfTrade)
                }
            };
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

        var rows = (await _provider.GetByDateRangeAsync(normalized, from, to, tradingMethod, subTypeOfTrade, cancellationToken)).ToArray();

        return new
        {
            symbol = normalized,
            from = from.ToString("yyyy-MM-dd"),
            to = to.ToString("yyyy-MM-dd"),
            tradingMethod,
            tradingMethodLabel = PsimsLabels.TradingMethod(tradingMethod),
            subTypeOfTrade,
            subTypeOfTradeLabel = PsimsLabels.TradeReportSubType(subTypeOfTrade),
            count = rows.Length,
            rows
        };
    }
}
