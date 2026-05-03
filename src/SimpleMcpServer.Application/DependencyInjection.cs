using Microsoft.Extensions.DependencyInjection;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Application.Services;
using SimpleMcpServer.Application.Tools;

namespace SimpleMcpServer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IDataTool, FundamentalLookupTool>();
        services.AddSingleton<IDataTool, CustomerLookupTool>();
        services.AddSingleton<ToolRegistry>();
        services.AddSingleton<IRpcDispatcher, RpcDispatcher>();
        return services;
    }
}
