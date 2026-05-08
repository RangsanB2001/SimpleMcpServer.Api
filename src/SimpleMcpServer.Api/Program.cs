using ModelContextProtocol.Protocol;
using SimpleMcpServer.Application;
using SimpleMcpServer.Application.Serialization;
using SimpleMcpServer.Application.Tools;
using SimpleMcpServer.Infrastructure;

const string McpEndpoint = "/mcp";
var toolsAssembly = typeof(FundamentalLookupTool).Assembly;
var jsonOptions = McpJsonOptions.Default;

if (args.Contains("--stdio", StringComparer.OrdinalIgnoreCase))
{
    var hostBuilder = Host.CreateApplicationBuilder(args);
    hostBuilder.Logging.ClearProviders();

    hostBuilder.Services
        .AddApplication()
        .AddInfrastructure()
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(toolsAssembly, jsonOptions)
        .WithPromptsFromAssembly(toolsAssembly, jsonOptions)
        .WithListResourcesHandler((_, _) => ValueTask.FromResult(new ListResourcesResult()))
        .WithListResourceTemplatesHandler((_, _) => ValueTask.FromResult(new ListResourceTemplatesResult()));

    await hostBuilder.Build().RunAsync();
    return;
}

var webBuilder = WebApplication.CreateBuilder(args);

webBuilder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithToolsFromAssembly(toolsAssembly, jsonOptions)
    .WithPromptsFromAssembly(toolsAssembly, jsonOptions)
    .WithListResourcesHandler((_, _) => ValueTask.FromResult(new ListResourcesResult()))
    .WithListResourceTemplatesHandler((_, _) => ValueTask.FromResult(new ListResourceTemplatesResult()));

var app = webBuilder.Build();
app.MapMcp(McpEndpoint);
await app.RunAsync();
