using System.Text.Json;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.JsonRpc;

namespace SimpleMcpServer.Application.Tools;

public class CustomerLookupTool : IDataTool
{
    public string Name => "customer_lookup";

    public string Description => "Lookup customer information by customer id";

    public object InputSchema => new
    {
        type = "object",
        properties = new
        {
            customer_id = new { type = "integer" }
        },
        required = new[] { "customer_id" }
    };

    public Task<object> ExecuteAsync(
        IReadOnlyDictionary<string, JsonElement>? arguments,
        CancellationToken cancellationToken = default)
    {
        if (arguments is null ||
            !arguments.TryGetValue("customer_id", out var idElement) ||
            !idElement.TryGetInt32(out var id))
        {
            throw new JsonRpcException(
                JsonRpcErrorCodes.InvalidParams,
                "customer_id (integer) is required");
        }

        return Task.FromResult<object>(new
        {
            id,
            name = "Enterprise Customer",
            tier = "Gold"
        });
    }
}
