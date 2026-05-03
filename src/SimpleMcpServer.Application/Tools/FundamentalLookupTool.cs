using System.Text.Json;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.JsonRpc;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Application.Tools;

public class FundamentalLookupTool : IDataTool
{
    private readonly IDataProvider<FundamentalData> _provider;

    public FundamentalLookupTool(IDataProvider<FundamentalData> provider)
    {
        _provider = provider;
    }

    public string Name => "fundamental_lookup";

    public string Description => "Get stock fundamental financial data by symbol";

    public object InputSchema => new
    {
        type = "object",
        properties = new
        {
            symbol = new
            {
                type = "string",
                description = "Stock symbol"
            }
        },
        required = new[] { "symbol" }
    };

    public async Task<object> ExecuteAsync(
        IReadOnlyDictionary<string, JsonElement>? arguments,
        CancellationToken cancellationToken = default)
    {
        if (arguments is null ||
            !arguments.TryGetValue("symbol", out var symbolElement) ||
            symbolElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonRpcException(
                JsonRpcErrorCodes.InvalidParams,
                "symbol (string) is required");
        }

        var symbol = symbolElement.GetString()!;

        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new JsonRpcException(
                JsonRpcErrorCodes.InvalidParams,
                "symbol must not be empty");
        }

        var result = await _provider.GetByKeyAsync(symbol, cancellationToken);

        if (result is null)
        {
            throw new JsonRpcException(
                JsonRpcErrorCodes.InvalidParams,
                $"No fundamental data found for symbol '{symbol}'");
        }

        return result;
    }
}
