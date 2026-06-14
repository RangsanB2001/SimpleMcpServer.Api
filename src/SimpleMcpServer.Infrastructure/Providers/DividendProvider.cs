using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class DividendProvider : IDividendProvider
{
    private const string SelectColumns =
        "sec_name, news_ann_date, seq_of_ann, board_date, book_closing_date, record_date, " +
        "begin_x_date, ending_x_date, payment_date, dividend_type, dividend_flag, " +
        "dividend_price, share_dividend_ratio, alloted_shares, par_value, " +
        "source_of_dividend_payment, cancel_status, " +
        "tentative_share_dividend_ratio, ten_divi_pershare_flag, " +
        "ten_divi_price_pershare, ten_divi_price_pershare_end, " +
        "dv_begin_x_session_nt, url_under_corporate_action_news, " +
        "payment_date_flag";

    private readonly string _connectionString;

    public DividendProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<IEnumerable<Dividend>> GetBySymbolAsync(string symbol,int limit,CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM Dividend " +
            "WHERE sec_name = @Symbol " +
            "  AND (cancel_status IS NULL OR cancel_status <> 'C') " +
            "ORDER BY news_ann_date DESC, seq_of_ann DESC " +
            "LIMIT @Limit;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<Dividend>(new CommandDefinition(sql,new { Symbol = symbol, Limit = limit },cancellationToken: cancellationToken));
    }
}
