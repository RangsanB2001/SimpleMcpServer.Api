using System.Text.Json;

namespace SimpleMcpServer.Api.Models
{
    public class JsonRpcRequest
    {
        public string JsonRpc { get; set; } = "2.0";

        public string Method { get; set; } = string.Empty;

        public JsonElement? Params { get; set; }

        public int Id { get; set; }
    }
}
