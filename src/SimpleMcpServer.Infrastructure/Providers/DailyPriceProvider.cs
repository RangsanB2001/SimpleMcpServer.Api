using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class DailyPriceProvider : IDailyPriceProvider
{
    private const string SelectColumns =
        "date, sec_name, sec_id, mk_type, trading_method, sub_type_of_trade, " +
        "prior_date, prior, open, high, low, close, " +
        "last_bid, last_offer, transaction, volume, value, avg_price";

    private readonly string _connectionString;

    public DailyPriceProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<DailyPrice?> GetLatestAsync(string symbol, string tradingMethod, string? subTypeOfTrade, CancellationToken cancellationToken = default)
    {
        var subTypeFilter = subTypeOfTrade is null
            ? string.Empty
            : "AND sub_type_of_trade = @SubTypeOfTrade ";

        var sql =
            $"SELECT {SelectColumns} " +
            "FROM d_trade " +
            "WHERE sec_name = @Symbol AND trading_method = @TradingMethod " +
            subTypeFilter +
            "ORDER BY date DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<DailyPrice>(
            new CommandDefinition(sql,new { Symbol = symbol, TradingMethod = tradingMethod, SubTypeOfTrade = subTypeOfTrade },cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DailyPrice>> GetByDateRangeAsync(string symbol,DateTime fromDate,DateTime toDate,string tradingMethod,string? subTypeOfTrade,CancellationToken cancellationToken = default)
    {
        var subTypeFilter = subTypeOfTrade is null
            ? string.Empty
            : "AND sub_type_of_trade = @SubTypeOfTrade ";

        var sql =
            $"SELECT {SelectColumns} " +
            "FROM d_trade " +
            "WHERE sec_name = @Symbol AND trading_method = @TradingMethod " +
            subTypeFilter +
            "  AND date BETWEEN @From AND @To " +
            "ORDER BY date ASC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<DailyPrice>(
            new CommandDefinition(
                sql,
                new
                {
                    Symbol = symbol,
                    TradingMethod = tradingMethod,
                    SubTypeOfTrade = subTypeOfTrade,
                    From = fromDate.Date,
                    To = toDate.Date
                },
                cancellationToken: cancellationToken));
    }
}
