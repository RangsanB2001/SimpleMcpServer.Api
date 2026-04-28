namespace SimpleMcpServer.Api.Mcp
{
    public interface IDataTool
    {
        string Name { get; }
        string Description { get; }
        object InputSchema { get; }
        Task<object> ExecuteAsync(
            Dictionary<string, object>? arguments,
            CancellationToken cancellationToken = default);
    }
}
