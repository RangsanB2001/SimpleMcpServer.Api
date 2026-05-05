using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class ParChangeProvider : IParChangeProvider
{
    private const string SelectColumns =
        "sec_name, news_ann_date, seq_of_ann, board_date, " +
        "old_par_val, new_par_val, effect_date, change_par_type, cancel_status";

    private readonly string _connectionString;

    public ParChangeProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<IEnumerable<ParChange>> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM ChgPar " +
            "WHERE sec_name = @Symbol " +
            "  AND (cancel_status IS NULL OR cancel_status <> 'C') " +
            "ORDER BY effect_date DESC, news_ann_date DESC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<ParChange>(
            new CommandDefinition(
                sql,
                new { Symbol = symbol },
                cancellationToken: cancellationToken));
    }
}
