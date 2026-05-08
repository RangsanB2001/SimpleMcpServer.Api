using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class NvdrHoldingProvider : INvdrHoldingProvider
{
    private const string SelectColumns =
        "sec_name, sec_id, date_as_of, date_data, " +
        "no_share_inhand, avg_cost, p_volume, p_value";

    private readonly string _connectionString;

    public NvdrHoldingProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<NvdrHolding?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM Nvdr " +
            "WHERE sec_name = @Symbol " +
            "ORDER BY date_as_of DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<NvdrHolding>(
            new CommandDefinition(sql, new { Symbol = symbol }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<NvdrHolding>> GetByDateRangeAsync(string symbol, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM Nvdr " +
            "WHERE sec_name = @Symbol " +
            "  AND date_as_of BETWEEN @From AND @To " +
            "ORDER BY date_as_of ASC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<NvdrHolding>(
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
