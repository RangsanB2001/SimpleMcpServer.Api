using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class FinancialStatementProvider : IFinancialStatementProvider
{
    private const string SelectColumns =
        "sec_name, sec_id, fiscal, quarter, fin_state_type, adjust_fin_state, " +
        "date_as_of, accountid AS AccountId, amount, accu_amount";

    private readonly string _connectionString;

    public FinancialStatementProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<IEnumerable<FinancialStatementLine>> GetByPeriodAsync(
        string symbol,
        int fiscal,
        string quarter,
        string finStateType,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM FinStmtDet " +
            "WHERE sec_name = @Symbol " +
            "  AND fiscal = @Fiscal " +
            "  AND quarter = @Quarter " +
            "  AND fin_state_type = @FinStateType " +
            "  AND adjust_fin_state = 'O' " +
            "ORDER BY accountid;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<FinancialStatementLine>(
            new CommandDefinition(
                sql,
                new
                {
                    Symbol = symbol,
                    Fiscal = fiscal,
                    Quarter = quarter,
                    FinStateType = finStateType
                },
                cancellationToken: cancellationToken));
    }
}
