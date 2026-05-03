using SimpleMcpServer.Application.JsonRpc;

namespace SimpleMcpServer.Application.Abstractions;

public interface IRpcDispatcher
{
    Task<JsonRpcResponse> DispatchAsync(
        JsonRpcRequest request,
        CancellationToken cancellationToken = default);
}
