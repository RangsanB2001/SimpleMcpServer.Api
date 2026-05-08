using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class ShortPositionProvider : IShortPositionProvider
{
    private const string SelectColumns =
        "sec_id, sec_name, date, short_pos_outs_volume";

    private readonly string _connectionString;

    public ShortPositionProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<ShortPosition?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM shortpos " +
            "WHERE sec_name = @Symbol " +
            "ORDER BY date DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<ShortPosition>(
            new CommandDefinition(sql, new { Symbol = symbol }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ShortPosition>> GetByDateRangeAsync(string symbol, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM shortpos " +
            "WHERE sec_name = @Symbol " +
            "  AND date BETWEEN @From AND @To " +
            "ORDER BY date ASC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<ShortPosition>(
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
