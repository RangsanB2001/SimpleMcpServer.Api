using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class ParChangeTools
{
    private readonly IParChangeProvider _provider;

    public ParChangeTools(IParChangeProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "par_changes")]
    [Description(
        "Get par-value change history for a Thai SET/mai stock from the ChgPar table. " +
        "Returns stock splits ('S') and capital reductions ('R') with old/new par value and effective date. " +
        "Cancelled rows are excluded.")]
    public async Task<object> GetParChanges(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("symbol must not be empty", nameof(symbol));
        }

        var rows = (await _provider.GetBySymbolAsync(symbol, cancellationToken)).ToArray();

        return new
        {
            symbol,
            count = rows.Length,
            rows
        };
    }
}
