using Microsoft.AspNetCore.Mvc;
using SimpleMcpServer.Api.Models;
using SimpleMcpServer.Api.Services;

namespace SimpleMcpServer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class McpToolController : ControllerBase
    {
        private readonly ToolRegistry _toolRegistry;
        private readonly ILogger<McpToolController> _logger;

        public McpToolController(ToolRegistry toolRegistry, ILogger<McpToolController> logger)
        {
            _toolRegistry = toolRegistry;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle([FromBody] JsonRpcRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    error = "request is required"
                });
            }

            _logger.LogInformation(
                "RPC method: {Method}",
                request.Method);

            switch (request.Method)
            {
                case "tools/list":
                    return Ok(new
                    {
                        jsonrpc = "2.0",
                        id = request.Id,
                        result = _toolRegistry.ListTools()
                    });

                case "tools/call":
                    return await HandleToolCall(
                        request,
                        cancellationToken);

                default:
                    return BadRequest(new
                    {
                        jsonrpc = "2.0",
                        id = request.Id,
                        error = "unknown method"
                    });
            }
        }

        private async Task<IActionResult> HandleToolCall(JsonRpcRequest request, CancellationToken cancellationToken)
        {
            if (request.Params == null)
            {
                return BadRequest(new
                {
                    error = "params required"
                });
            }

            var p = request.Params.Value;

            if (!p.TryGetProperty("name", out var nameElement))
            {
                return BadRequest(new
                {
                    error = "tool name required"
                });
            }

            var toolName = nameElement.GetString();

            Dictionary<string, object>? arguments = null;

            if (p.TryGetProperty("arguments", out var args))
            {
                arguments = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(args.GetRawText());
            }

            var result = await _toolRegistry.CallToolAsync(
                toolName!,
                arguments,
                cancellationToken);

            return Ok(new
            {
                jsonrpc = "2.0",
                id = request.Id,
                result
            });
        }
    }
}
