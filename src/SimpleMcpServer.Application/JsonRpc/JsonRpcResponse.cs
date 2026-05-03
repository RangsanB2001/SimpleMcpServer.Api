using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleMcpServer.Application.JsonRpc;

public class JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? Error { get; set; }

    [JsonPropertyName("id")]
    public JsonElement? Id { get; set; }

    public static JsonRpcResponse Success(JsonElement? id, object? result) =>
        new() { Id = id, Result = result };

    public static JsonRpcResponse Failure(JsonElement? id, int code, string message, object? data = null) =>
        new()
        {
            Id = id,
            Error = new JsonRpcError
            {
                Code = code,
                Message = message,
                Data = data
            }
        };
}
