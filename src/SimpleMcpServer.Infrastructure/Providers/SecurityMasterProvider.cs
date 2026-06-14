using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class SecurityMasterProvider : ISecurityMasterProvider
{
    private const string SelectColumns =
        "sec_name, sec_id, com_id, " +
        "sec_fullname_th, sec_fullname_eng, " +
        "sec_type, mk_type, industry_no, sector_no, subsector_no, " +
        "no_author_shares, no_paidup_shares, no_issued_shares, no_listed_shares, " +
        "ipo_price, currency, par_val, " +
        "list_date, trading_date, issued_date, expired_date, " +
        "account_form, fiscal_year_enddate, " +
        "sec_list_status, delist_date, cause_del, " +
        "last_trading_date, last_exercise, " +
        "conver_ex_str_price, conver_ex_ratio";

    private readonly string _connectionString;

    public SecurityMasterProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<SecurityMaster?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM Compsec " +
            "WHERE sec_name = @Symbol " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<SecurityMaster>(
            new CommandDefinition(sql, new { Symbol = symbol }, cancellationToken: cancellationToken));
    }
}
