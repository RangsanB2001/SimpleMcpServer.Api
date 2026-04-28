using SimpleMcpServer.Api.Mcp;

namespace SimpleMcpServer.Api.Services
{
    public class ToolRegistry
    {
        private readonly IEnumerable<IDataTool> _tools;

        public ToolRegistry(IEnumerable<IDataTool> tools)
        {
            _tools = tools;
        }

        public IEnumerable<object> ListTools()
        {
            return _tools.Select(x => new
            {
                name = x.Name,
                description = x.Description,
                inputSchema = x.InputSchema
            });
        }

        public async Task<object> CallToolAsync(string name, Dictionary<string, object>? arguments, CancellationToken cancellationToken = default)
        {
            var tool = _tools.FirstOrDefault(x =>
                x.Name.Equals(name,
                    StringComparison.OrdinalIgnoreCase));

            if (tool == null)
            {
                return new
                {
                    success = false,
                    error = $"Tool '{name}' not found"
                };
            }

            return await tool.ExecuteAsync(arguments, cancellationToken);
        }
    }
}

