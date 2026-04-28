namespace SimpleMcpServer.Api.Models
{
    public class JsonRpcResponse
    {
        public string JsonRpc { get; set; } = "2.0";

        public object? Result { get; set; }

        public JsonRpcError? Error { get; set; }

        public int Id { get; set; }
    }
}
