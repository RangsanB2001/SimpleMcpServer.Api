using System.Text.Json;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.JsonRpc;

namespace SimpleMcpServer.Application.Services;

public class ToolRegistry
{
    private readonly IReadOnlyDictionary<string, IDataTool> _tools;

    public ToolRegistry(IEnumerable<IDataTool> tools)
    {
        _tools = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<object> ListTools() =>
        _tools.Values.Select(t => new
        {
            name = t.Name,
            description = t.Description,
            inputSchema = t.InputSchema
        });

    public Task<object> CallToolAsync(
        string name,
        IReadOnlyDictionary<string, JsonElement>? arguments,
        CancellationToken cancellationToken = default)
    {
        if (!_tools.TryGetValue(name, out var tool))
        {
            throw new JsonRpcException(
                JsonRpcErrorCodes.MethodNotFound,
                $"Tool '{name}' not found");
        }

        return tool.ExecuteAsync(arguments, cancellationToken);
    }
}
