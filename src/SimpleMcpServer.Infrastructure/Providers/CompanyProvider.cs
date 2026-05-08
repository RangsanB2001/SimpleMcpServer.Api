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
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<CompanyProfile?> GetBySymbolAsync(string symbol,CancellationToken cancellationToken = default)
    {
        const string sql =
            "SELECT b.sec_name, " +
            "       b.com_id, " +
            "       c.com_name_th, c.com_name_eng, " +
            "       c.com_addr_th, c.com_addr_eng, c.zipcode, " +
            "       c.tel_num, c.fax_num, c.url, " +
            "       c.establish_date, c.com_type, " +
            "       b.business_type_th, b.business_type_en, " +
            "       cg.rating  AS cg_score, " +
            "       cg.rating_flag AS cg_rating_flag, " +
            "       cg.date_as_of  AS cg_as_of, " +
            "       cac.rating AS cac_flag, " +
            "       cac.date_as_of AS cac_as_of, " +
            "       esg.rating AS esg_rating, " +
            "       esg.date_as_of AS esg_as_of, " +
            "       c.divide_pol_th, c.divide_pol_eng, " +
            "       c.dividend_policy_file_name_th, c.dividend_policy_file_name_en, " +
            "       c.juristic_person_registration, " +
            "       c.issuer_guarantor_credit_rating, c.issuer_guarantor_rating_outlook, " +
            "       c.credit_rating_agency_th, c.credit_rating_agency_eng, " +
            "       c.date_as_of " +
            "FROM business b " +
            "INNER JOIN Company c ON c.com_id = b.com_id " +
            "LEFT JOIN Comprating cg  ON cg.company_id  = b.com_id AND cg.rating_type  = 'CG' " +
            "                        AND cg.date_as_of  = (SELECT MAX(date_as_of) FROM Comprating WHERE company_id = b.com_id AND rating_type = 'CG') " +
            "LEFT JOIN Comprating cac ON cac.company_id = b.com_id AND cac.rating_type = 'CAC' " +
            "                        AND cac.date_as_of = (SELECT MAX(date_as_of) FROM Comprating WHERE company_id = b.com_id AND rating_type = 'CAC') " +
            "LEFT JOIN Comprating esg ON esg.company_id = b.com_id AND esg.rating_type = 'ESG' " +
            "                        AND esg.date_as_of = (SELECT MAX(date_as_of) FROM Comprating WHERE company_id = b.com_id AND rating_type = 'ESG') " +
            "WHERE b.sec_name = @Symbol " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<CompanyProfile>(new CommandDefinition(sql,new { Symbol = symbol },cancellationToken: cancellationToken));
    }
}
