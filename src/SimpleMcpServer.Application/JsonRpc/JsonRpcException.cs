namespace SimpleMcpServer.Application.JsonRpc;

public class JsonRpcException : Exception
{
    public int Code { get; }

    public object? ErrorData { get; }

    public JsonRpcException(int code, string message, object? errorData = null)
        : base(message)
    {
        Code = code;
        ErrorData = errorData;
    }
}
