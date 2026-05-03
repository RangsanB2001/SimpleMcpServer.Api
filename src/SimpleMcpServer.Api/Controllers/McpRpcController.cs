using Microsoft.AspNetCore.Mvc;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.JsonRpc;

namespace SimpleMcpServer.Api.Controllers;

[ApiController]
[Route("api/rpc")]
public class McpRpcController : ControllerBase
{
    private readonly IRpcDispatcher _dispatcher;
    private readonly ILogger<McpRpcController> _logger;

    public McpRpcController(IRpcDispatcher dispatcher, ILogger<McpRpcController> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public const string RequestIdItemKey = "jsonrpc.id";

    [HttpPost]
    public Task<JsonRpcResponse> Handle(
        [FromBody] JsonRpcRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new JsonRpcException(
                JsonRpcErrorCodes.InvalidRequest,
                "request body required");
        }

        if (request.Id.HasValue)
        {
            HttpContext.Items[RequestIdItemKey] = request.Id.Value;
        }

        _logger.LogInformation("RPC method: {Method}", request.Method);

        return _dispatcher.DispatchAsync(request, cancellationToken);
    }
}
