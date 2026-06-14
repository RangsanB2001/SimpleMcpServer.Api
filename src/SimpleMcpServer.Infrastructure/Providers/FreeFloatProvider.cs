using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class FreeFloatProvider : IFreeFloatProvider
{
    private const string SelectColumns =
        "sec_name, sec_id, shareholder_as_of_date, " +
        "free_float AS MinorShareholders, per_free_float, book_close_type";

    private readonly string _connectionString;

    public FreeFloatProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<FreeFloat?> GetLatestAsync(string symbol, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM freeflt " +
            "WHERE sec_name = @Symbol " +
            "ORDER BY shareholder_as_of_date DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<FreeFloat>(
            new CommandDefinition(sql, new { Symbol = symbol }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<FreeFloat>> GetHistoryAsync(string symbol, int limit, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM freeflt " +
            "WHERE sec_name = @Symbol " +
            "ORDER BY shareholder_as_of_date DESC " +
            "LIMIT @Limit;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<FreeFloat>(
            new CommandDefinition(sql, new { Symbol = symbol, Limit = limit }, cancellationToken: cancellationToken));
    }
}
