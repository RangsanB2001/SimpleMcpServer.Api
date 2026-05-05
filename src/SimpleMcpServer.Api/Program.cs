using SimpleMcpServer.Application;
using SimpleMcpServer.Application.Tools;
using SimpleMcpServer.Infrastructure;

const string McpEndpoint = "/mcp";
var toolsAssembly = typeof(FundamentalLookupTool).Assembly;

if (args.Contains("--stdio", StringComparer.OrdinalIgnoreCase))
{
    var hostBuilder = Host.CreateApplicationBuilder(args);
    hostBuilder.Logging.ClearProviders();

    hostBuilder.Services
        .AddApplication()
        .AddInfrastructure()
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(toolsAssembly)
        .WithPromptsFromAssembly(toolsAssembly);

    await hostBuilder.Build().RunAsync();
    return;
}

var webBuilder = WebApplication.CreateBuilder(args);

webBuilder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithToolsFromAssembly(toolsAssembly)
    .WithPromptsFromAssembly(toolsAssembly);

var app = webBuilder.Build();
app.MapMcp(McpEndpoint);
await app.RunAsync();
