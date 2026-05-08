using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class DailyStatProvider : IDailyStatProvider
{
    private const string SelectColumns =
        "date, sec_name, sec_id, mk_type, " +
        "industry_no, sector_no, subsector_no, " +
        "sec_type, status, benefit, " +
        "listed_share, earning_per_share, book_net_value, " +
        "quarter_period, fin_state, earning_date, " +
        "divide_per_share, period_dividend, enddate_of_dividend, " +
        "pe, pbv, dividend_yield, par_val, mk_cap, turn_ratio_by_vol, " +
        "npg_flag, acc_dividend_per_share, acc_no_of_payment_per_year, dividend_payout_ratio, " +
        "notice_pending_receive_flag, non_compliance_flag, stabilization_flag, " +
        "call_market_flag, caution_flag, " +
        "twelve_month_dividend_yield, peg_ratio";

    private readonly string _connectionString;

    public DailyStatProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<DailyStat?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM d_stat " +
            "WHERE sec_name = @Symbol " +
            "ORDER BY date DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<DailyStat>(
            new CommandDefinition(sql, new { Symbol = symbol }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DailyStat>> GetByDateRangeAsync(string symbol, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM d_stat " +
            "WHERE sec_name = @Symbol " +
            "  AND date BETWEEN @From AND @To " +
            "ORDER BY date ASC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<DailyStat>(
            new CommandDefinition(
                sql,
                new
                {
                    Symbol = symbol,
                    From = fromDate.Date,
                    To = toDate.Date
                },
                cancellationToken: cancellationToken));
    }
}
