using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class DailyPriceProvider : IDailyPriceProvider
{
    private const string SelectColumns =
        "date, sec_name, sec_id, mk_type, trading_method, " +
        "prior, open, high, low, close, volume, value, avg_price";

    private const string AutoMatchingTradingMethod = "A";

    private readonly string _connectionString;

    public DailyPriceProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<DailyPrice?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM d_trade " +
            "WHERE sec_name = @Symbol AND trading_method = @TradingMethod " +
            "ORDER BY date DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<DailyPrice>(
            new CommandDefinition(
                sql,
                new { Symbol = symbol, TradingMethod = AutoMatchingTradingMethod },
                cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DailyPrice>> GetByDateRangeAsync(
        string symbol,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM d_trade " +
            "WHERE sec_name = @Symbol AND trading_method = @TradingMethod " +
            "  AND date BETWEEN @From AND @To " +
            "ORDER BY date ASC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<DailyPrice>(
            new CommandDefinition(
                sql,
                new
                {
                    Symbol = symbol,
                    TradingMethod = AutoMatchingTradingMethod,
                    From = fromDate.Date,
                    To = toDate.Date
                },
                cancellationToken: cancellationToken));
    }
}
