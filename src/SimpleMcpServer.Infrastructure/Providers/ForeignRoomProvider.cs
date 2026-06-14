using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class ForeignRoomProvider : IForeignRoomProvider
{
    private const string SelectColumns =
        "sec_name, sec_id, date_as_of, " +
        "p_foreign_limit, p_spforeign_limit, " +
        "tot_share, tot_foreignshares, " +
        "foreign_room_shares, foreign_queue_shares, no_share_available, " +
        "tot_holdshares, percent_foreign_queue, percent_foreign_available, " +
        "spforeign_limit_flag";

    private readonly string _connectionString;

    public ForeignRoomProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<ForeignRoom?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM F_Room " +
            "WHERE sec_name = @Symbol " +
            "ORDER BY date_as_of DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<ForeignRoom>(
            new CommandDefinition(sql, new { Symbol = symbol }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ForeignRoom>> GetByDateRangeAsync(string symbol, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM F_Room " +
            "WHERE sec_name = @Symbol " +
            "  AND date_as_of BETWEEN @From AND @To " +
            "ORDER BY date_as_of ASC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<ForeignRoom>(
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
