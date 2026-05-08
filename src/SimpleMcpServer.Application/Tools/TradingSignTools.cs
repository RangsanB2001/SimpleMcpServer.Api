using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class TradingSignTools
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 100;

    private readonly ITradingSignProvider _provider;

    public TradingSignTools(ITradingSignProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "trading_signs")]
    [Description(
        "Get trading sign postings (Sign table) for a Thai SET/mai stock. " +
        "Sign codes: NP=Notice Pending, NR=Notice Received, SP=Suspension, H=Halt, " +
        "C=Caution, CM=Call Market, ST=Stabilization, NC=Non-Compliance. " +
        "Pass mode='active' (default) for currently posted signs, or mode='history' for the most recent N postings. " +
        "Each row includes posting/lifting dates and news file names; for the reason behind each sign, use trading_sign_reasons.")]
    public async Task<object> GetTradingSigns(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("'active' (currently posted) or 'history' (recent postings). Default 'active'.")] string mode = "active",
        [Description("History limit (1-100, used only when mode='history'). Default 20.")] int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var normalized = SymbolHelpers.Normalize(symbol);

        if (mode is not ("active" or "history"))
        {
            throw new ArgumentException("mode must be 'active' or 'history'", nameof(mode));
        }

        if (mode == "history" && (limit < 1 || limit > MaxLimit))
        {
            throw new ArgumentException($"limit must be between 1 and {MaxLimit}", nameof(limit));
        }

        var rows = mode == "active"
            ? (await _provider.GetActiveAsync(normalized, cancellationToken)).ToArray()
            : (await _provider.GetHistoryAsync(normalized, limit, cancellationToken)).ToArray();

        return new
        {
            symbol = normalized,
            mode,
            count = rows.Length,
            rows = rows.Select(r => new
            {
                row = r,
                decoded = new { sign = PsimsLabels.Sign(r.Sign) }
            }).ToArray()
        };
    }

    [McpServerTool(Name = "trading_sign_reasons")]
    [Description(
        "Get the detailed reason(s) for a specific trading-sign posting from the sign_det table. " +
        "Provide the symbol, the sign code, and the sign posting date (returned by trading_signs). " +
        "Reason code list — see Reason Code of Sign Posting reference (PSIMS Datadic).")]
    public async Task<object> GetTradingSignReasons(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("Sign code (e.g. SP, NP, C, NC, NR, ST, CM, H).")] string sign,
        [Description("Sign posting date in yyyy-MM-dd (matches sign_pos_date returned by trading_signs).")] string signPosDate,
        CancellationToken cancellationToken = default)
    {
        var normalized = SymbolHelpers.Normalize(symbol);

        if (string.IsNullOrWhiteSpace(sign))
        {
            throw new ArgumentException("sign must not be empty", nameof(sign));
        }

        if (!DateTime.TryParse(signPosDate, out var posDate))
        {
            throw new ArgumentException("signPosDate must be a valid date in yyyy-MM-dd format", nameof(signPosDate));
        }

        var rows = (await _provider.GetReasonsAsync(normalized, posDate, sign, cancellationToken)).ToArray();

        return new
        {
            symbol = normalized,
            sign,
            signLabel = PsimsLabels.Sign(sign),
            signPosDate = posDate.ToString("yyyy-MM-dd"),
            count = rows.Length,
            rows = rows.Select(r => new
            {
                row = r,
                decoded = new { sign = PsimsLabels.Sign(r.Sign) }
            }).ToArray()
        };
    }
}
