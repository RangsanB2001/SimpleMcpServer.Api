using System.Text.Json;
using SimpleMcpServer.Api.Controllers;
using SimpleMcpServer.Application.JsonRpc;

namespace SimpleMcpServer.Api.Middleware;

public class JsonRpcExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JsonRpcExceptionMiddleware> _logger;

    public JsonRpcExceptionMiddleware(
        RequestDelegate next,
        ILogger<JsonRpcExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (JsonRpcException ex)
        {
            _logger.LogWarning(ex, "JSON-RPC error: {Message}", ex.Message);
            await WriteErrorAsync(context, ex.Code, ex.Message, ex.ErrorData);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "JSON parse error");
            await WriteErrorAsync(context, JsonRpcErrorCodes.ParseError, "Parse error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            await WriteErrorAsync(context, JsonRpcErrorCodes.InternalError, "Internal error");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int code,
        string message,
        object? data = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";

        var response = JsonRpcResponse.Failure(GetRequestId(context), code, message, data);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static JsonElement? GetRequestId(HttpContext context)
    {
        if (context.Items.TryGetValue(McpRpcController.RequestIdItemKey, out var value) &&
            value is JsonElement element)
        {
            return element;
        }

        return null;
    }
}
