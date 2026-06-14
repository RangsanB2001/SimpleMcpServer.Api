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
        "date_as_of, accountid AS AccountId, amount, accu_amount, balance_flag";

    private const string HeaderSelectColumns =
        "sec_name, sec_id, fin_state_type, fiscal, quarter, adjust_fin_state, " +
        "date_as_of, account_form, currency_fin_state, " +
        "start_date_fiscal_year, start_date_fin_state, " +
        "statement_fin_position_flag, income_statement_flag, cash_flow_flag, " +
        "status_fin_state_3m, status_fin_state_acc, " +
        "status_statement_financial_position, status_statement_cashflow, " +
        "date_income_statement, date_statement_financial_position, date_statement_cashflow, " +
        "startdate_statement_cashflow, " +
        "news_filename_th, news_filename_en";

    private readonly string _connectionString;

    public FinancialStatementProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<IEnumerable<FinancialStatementLine>> GetByPeriodAsync(string symbol,int fiscal,string quarter,string finStateType,string adjustFinState,CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM FinStmtDet " +
            "WHERE sec_name = @Symbol " +
            "  AND fiscal = @Fiscal " +
            "  AND quarter = @Quarter " +
            "  AND fin_state_type = @FinStateType " +
            "  AND adjust_fin_state = @AdjustFinState " +
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
                    FinStateType = finStateType,
                    AdjustFinState = adjustFinState
                },
                cancellationToken: cancellationToken));
    }

    public async Task<FinancialStatementHeader?> GetHeaderByPeriodAsync(string symbol,int fiscal,string quarter,string finStateType,string adjustFinState,CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {HeaderSelectColumns} " +
            "FROM FinStmt " +
            "WHERE sec_name = @Symbol " +
            "  AND fiscal = @Fiscal " +
            "  AND quarter = @Quarter " +
            "  AND fin_state_type = @FinStateType " +
            "  AND adjust_fin_state = @AdjustFinState " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<FinancialStatementHeader>(
            new CommandDefinition(
                sql,
                new
                {
                    Symbol = symbol,
                    Fiscal = fiscal,
                    Quarter = quarter,
                    FinStateType = finStateType,
                    AdjustFinState = adjustFinState
                },
                cancellationToken: cancellationToken));
    }
}
