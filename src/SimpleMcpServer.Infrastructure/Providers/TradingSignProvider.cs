using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class TradingSignProvider : ITradingSignProvider
{
    private const string SignSelectColumns =
        "sec_name, sec_id, sign, sign_pos_date, sign_lift_date, " +
        "sign_pos_newsfname_th, sign_pos_newsfname_eng, " +
        "sign_lift_newsfname_th, sign_lift_newsfname_eng";

    private const string DetailSelectColumns =
        "sec_name, sec_id, sign, sign_pos_date, sign_pos_reason_seq_no, sign_pos_reason, " +
        "reason_start_date, reason_lift_date, " +
        "reason_detail_quarter_of_financial_statement, reason_detail_as_of_date_of_financial_statement, " +
        "reason_detail_description_th, reason_detail_description_en";

    private readonly string _connectionString;

    public TradingSignProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<IEnumerable<TradingSign>> GetActiveAsync(string symbol, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SignSelectColumns} " +
            "FROM Sign " +
            "WHERE sec_name = @Symbol " +
            "  AND (sign_lift_date IS NULL OR sign_lift_date > NOW()) " +
            "ORDER BY sign_pos_date DESC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<TradingSign>(
            new CommandDefinition(sql, new { Symbol = symbol }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<TradingSign>> GetHistoryAsync(string symbol, int limit, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SignSelectColumns} " +
            "FROM Sign " +
            "WHERE sec_name = @Symbol " +
            "ORDER BY sign_pos_date DESC " +
            "LIMIT @Limit;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<TradingSign>(
            new CommandDefinition(sql, new { Symbol = symbol, Limit = limit }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<TradingSignDetail>> GetReasonsAsync(string symbol, DateTime signPosDate, string sign, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {DetailSelectColumns} " +
            "FROM sign_det " +
            "WHERE sec_name = @Symbol " +
            "  AND sign = @Sign " +
            "  AND sign_pos_date = @SignPosDate " +
            "ORDER BY sign_pos_reason_seq_no ASC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<TradingSignDetail>(
            new CommandDefinition(
                sql,
                new { Symbol = symbol, Sign = sign, SignPosDate = signPosDate },
                cancellationToken: cancellationToken));
    }
}
