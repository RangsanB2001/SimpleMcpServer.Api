using System.Text.Json;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.JsonRpc;

namespace SimpleMcpServer.Application.Services;

public class RpcDispatcher : IRpcDispatcher
{
    private readonly ToolRegistry _toolRegistry;

    public RpcDispatcher(ToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public async Task<JsonRpcResponse> DispatchAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken = default)
    {
        switch (request.Method)
        {
            case "tools/list":
                return JsonRpcResponse.Success(
                    request.Id,
                    new { tools = _toolRegistry.ListTools() });

            case "tools/call":
                var result = await HandleToolCallAsync(request, cancellationToken);
                return JsonRpcResponse.Success(request.Id, result);

            default:
                throw new JsonRpcException(
                    JsonRpcErrorCodes.MethodNotFound,
                    $"Method '{request.Method}' not found");
        }
    }

    private Task<object> HandleToolCallAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Params is not { ValueKind: JsonValueKind.Object } p)
        {
            throw new JsonRpcException(
                JsonRpcErrorCodes.InvalidParams,
                "params object required");
        }

        if (!p.TryGetProperty("name", out var nameElement) ||
            nameElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonRpcException(
                JsonRpcErrorCodes.InvalidParams,
                "tool name (string) required");
        }

        var toolName = nameElement.GetString()!;

        IReadOnlyDictionary<string, JsonElement>? arguments = null;

        if (p.TryGetProperty("arguments", out var argsElement) &&
            argsElement.ValueKind == JsonValueKind.Object)
        {
            arguments = argsElement
                .EnumerateObject()
                .ToDictionary(prop => prop.Name, prop => prop.Value);
        }

        return _toolRegistry.CallToolAsync(toolName, arguments, cancellationToken);
    }
}
