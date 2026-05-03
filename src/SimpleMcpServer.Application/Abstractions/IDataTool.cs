using System.Text.Json;

namespace SimpleMcpServer.Application.Abstractions;

public interface IDataTool
{
    string Name { get; }

    string Description { get; }

    object InputSchema { get; }

    Task<object> ExecuteAsync(
        IReadOnlyDictionary<string, JsonElement>? arguments,
        CancellationToken cancellationToken = default);
}
