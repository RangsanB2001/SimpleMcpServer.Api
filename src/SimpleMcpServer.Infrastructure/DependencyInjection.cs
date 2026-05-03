using Microsoft.Extensions.DependencyInjection;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;
using SimpleMcpServer.Infrastructure.Providers;

namespace SimpleMcpServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDataProvider<FundamentalData>, FundamentalProvider>();
        return services;
    }
}
