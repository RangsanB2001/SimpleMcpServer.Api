using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class FreeFloatTools
{
    private const int DefaultLimit = 5;
    private const int MaxLimit = 50;

    private readonly IFreeFloatProvider _provider;

    public FreeFloatTools(IFreeFloatProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "free_float")]
    [Description(
        "Get free-float (minor shareholder) data for a Thai SET/mai stock from the freeflt table. " +
        "Returns the number of minor shareholders, % free float, and book closing type " +
        "(XM=Meeting, IPO=Initial Public Offering, BRS=Business Restructure). " +
        "Returns the latest snapshot by default; pass limit > 1 for the most recent N snapshots.")]
    public async Task<object> GetFreeFloat(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("How many recent snapshots to return (1-50, default 5).")] int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var normalized = SymbolHelpers.Normalize(symbol);

        if (limit < 1 || limit > MaxLimit)
        {
            throw new ArgumentException($"limit must be between 1 and {MaxLimit}", nameof(limit));
        }

        if (limit == 1)
        {
            var latest = await _provider.GetLatestAsync(normalized, cancellationToken);

            if (latest is null)
            {
                throw new InvalidOperationException($"No free float data found for symbol '{normalized}'");
            }

            return latest;
        }

        var rows = (await _provider.GetHistoryAsync(normalized, limit, cancellationToken)).ToArray();

        return new
        {
            symbol = normalized,
            count = rows.Length,
            rows
        };
    }
}
