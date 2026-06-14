using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class FundamentalProvider : IFundamentalProvider
{
    private const string SelectColumns =
        "sec_id, sec_name, fiscal, quarter, fs_period, last_price, " +
        "pe, pbv, mk_cap, listed_share, " +
        "earning_per_share, book_net_value, divide_per_share, dividend_yield, " +
        "net_profit_cons, roe, roa, roe_last4q, roa_last4q, " +
        "total_asset, total_equity, liabilities, cash, " +
        "debt_equity_ratio, gearing, " +
        "ebitda, paid_up, " +
        "operating_cash_flow, investment_cash_flow, financing_cash_flow, " +
        "gross_margin, oper_profit, net_profit, expense, " +
        "enterprise_value, dividend_coverage, price_cash_ratio, cash_earning_ratio, " +
        "eps_4q, pe_4q, diff_eps, " +
        "total_asset_turnover, working_capital, fixed_asset_turnover, temp_invest, " +
        "timestamp";

    private readonly string _connectionString;

    public FundamentalProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
    }

    public Task<FundamentalData?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        GetByPeriodAsync(key, fiscal: null, quarter: null, cancellationToken);

    public async Task<FundamentalData?> GetByPeriodAsync(string symbol, int? fiscal, string? quarter, CancellationToken cancellationToken = default)
    {
        var sql =
            $"SELECT {SelectColumns} " +
            "FROM stock_quarter " +
            "WHERE sec_name = @Symbol " +
            (fiscal.HasValue ? "AND fiscal = @Fiscal " : string.Empty) +
            (!string.IsNullOrWhiteSpace(quarter) ? "AND quarter = @Quarter " : string.Empty) +
            "ORDER BY fs_period DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<FundamentalData>(new CommandDefinition(sql,new { Symbol = symbol, Fiscal = fiscal, Quarter = quarter },cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<FundamentalData>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM stock_quarter " +
            "ORDER BY sec_name, fs_period DESC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<FundamentalData>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
