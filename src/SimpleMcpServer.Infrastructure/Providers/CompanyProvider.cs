using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class CompanyProvider : ICompanyProvider
{
    private readonly string _connectionString;

    public CompanyProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<CompanyProfile?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            "SELECT b.sec_name, " +
            "       b.com_id, " +
            "       c.com_name_th, c.com_name_eng, " +
            "       c.com_addr_th, c.com_addr_eng, c.zipcode, " +
            "       c.tel_num, c.fax_num, c.email, c.url, " +
            "       c.establish_date, c.com_type, " +
            "       b.business_type_th, b.business_type_en, " +
            "       b.cg_score, b.cac_flag, " +
            "       c.divide_pol_th, c.divide_pol_eng " +
            "FROM business b " +
            "INNER JOIN Company c ON c.com_id = b.com_id " +
            "WHERE b.sec_name = @Symbol " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<CompanyProfile>(
            new CommandDefinition(
                sql,
                new { Symbol = symbol },
                cancellationToken: cancellationToken));
    }
}
