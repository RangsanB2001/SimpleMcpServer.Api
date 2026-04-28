namespace SimpleMcpServer.Api.Models
{
    public class JsonRpcError
    {
        public int Code { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
