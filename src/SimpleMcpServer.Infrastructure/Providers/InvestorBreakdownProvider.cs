using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Entities;

namespace SimpleMcpServer.Infrastructure.Providers;

public class InvestorBreakdownProvider : IInvestorBreakdownProvider
{
    private const string SelectColumns =
        "date, mk_type, industry_no, sector_no, subsector_no, " +
        "buy_tran_cust, sell_tran_cust, buy_vol_cust, sell_vol_cust, buy_val_cust, sell_val_cust, " +
        "buy_tran_insti, sell_tran_insti, buy_vol_insti, sell_vol_insti, buy_val_insti, sell_val_insti, " +
        "buy_tran_foreigner, sell_tran_foreigner, buy_vol_foreigner, sell_vol_foreigner, buy_val_foreigner, sell_val_foreigner, " +
        "buy_tran_broker, sell_tran_broker, buy_vol_broker, sell_vol_broker, buy_val_broker, sell_val_broker";

    private const int MarketTotalSectorSet = 99;

    private const int MarketTotalSectorMai = 98;

    private readonly string _connectionString;

    public InvestorBreakdownProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<InvestorBreakdown?> GetLatestAsync(string marketType, CancellationToken cancellationToken = default)
    {
        var sectorNo = SectorNoFor(marketType);

        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM D_Cust " +
            "WHERE mk_type = @MkType " +
            "  AND sector_no = @SectorNo " +
            "ORDER BY date DESC " +
            "LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<InvestorBreakdown>(
            new CommandDefinition(sql, new { MkType = marketType, SectorNo = sectorNo }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<InvestorBreakdown>> GetByDateRangeAsync(string marketType, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var sectorNo = SectorNoFor(marketType);

        const string sql =
            $"SELECT {SelectColumns} " +
            "FROM D_Cust " +
            "WHERE mk_type = @MkType " +
            "  AND sector_no = @SectorNo " +
            "  AND date BETWEEN @From AND @To " +
            "ORDER BY date ASC;";

        await using var connection = new MySqlConnection(_connectionString);

        return await connection.QueryAsync<InvestorBreakdown>(
            new CommandDefinition(
                sql,
                new
                {
                    MkType = marketType,
                    SectorNo = sectorNo,
                    From = fromDate.Date,
                    To = toDate.Date
                },
                cancellationToken: cancellationToken));
    }

    private static int SectorNoFor(string marketType) => marketType switch
    {
        "A" => MarketTotalSectorSet,
        "S" => MarketTotalSectorMai,
        _ => throw new ArgumentException("marketType must be 'A' (SET) or 'S' (mai)", nameof(marketType))
    };
}
