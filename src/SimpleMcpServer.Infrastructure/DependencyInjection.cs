using Dapper;
using Microsoft.Extensions.DependencyInjection;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;
using SimpleMcpServer.Infrastructure.Providers;

namespace SimpleMcpServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddSingleton<FundamentalProvider>();
        services.AddSingleton<IFundamentalProvider>(sp => sp.GetRequiredService<FundamentalProvider>());
        services.AddSingleton<IDataProvider<FundamentalData>>(sp => sp.GetRequiredService<FundamentalProvider>());

        services.AddSingleton<IDailyPriceProvider, DailyPriceProvider>();
        services.AddSingleton<IDailyStatProvider, DailyStatProvider>();
        services.AddSingleton<ISecurityMasterProvider, SecurityMasterProvider>();
        services.AddSingleton<IForeignRoomProvider, ForeignRoomProvider>();
        services.AddSingleton<INvdrHoldingProvider, NvdrHoldingProvider>();
        services.AddSingleton<IShortPositionProvider, ShortPositionProvider>();
        services.AddSingleton<ITradingSignProvider, TradingSignProvider>();
        services.AddSingleton<IFreeFloatProvider, FreeFloatProvider>();
        services.AddSingleton<IInvestorBreakdownProvider, InvestorBreakdownProvider>();
        services.AddSingleton<IDividendProvider, DividendProvider>();
        services.AddSingleton<IParChangeProvider, ParChangeProvider>();
        services.AddSingleton<ICompanyProvider, CompanyProvider>();
        services.AddSingleton<IFinancialStatementProvider, FinancialStatementProvider>();

        return services;
    }
}
