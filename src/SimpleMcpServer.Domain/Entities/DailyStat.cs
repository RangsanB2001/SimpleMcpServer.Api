namespace SimpleMcpServer.Domain.Entities;

public class DailyStat
{
    public DateTime Date { get; set; }

    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public string? MkType { get; set; }

    public int? IndustryNo { get; set; }

    public int? SectorNo { get; set; }

    public int? SubsectorNo { get; set; }

    public string? SecType { get; set; }

    public string? Status { get; set; }

    public string? Benefit { get; set; }

    public decimal? ListedShare { get; set; }

    public decimal? EarningPerShare { get; set; }

    public decimal? BookNetValue { get; set; }

    public int? QuarterPeriod { get; set; }

    public DateTime? FinState { get; set; }

    public DateTime? EarningDate { get; set; }

    public decimal? DividePerShare { get; set; }

    public int? PeriodDividend { get; set; }

    public DateTime? EnddateOfDividend { get; set; }

    public decimal? Pe { get; set; }

    public decimal? Pbv { get; set; }

    public decimal? DividendYield { get; set; }

    public decimal? ParVal { get; set; }

    public decimal? MkCap { get; set; }

    public decimal? TurnRatioByVol { get; set; }

    public string? NpgFlag { get; set; }

    public decimal? AccDividendPerShare { get; set; }

    public int? AccNoOfPaymentPerYear { get; set; }

    public decimal? DividendPayoutRatio { get; set; }

    public string? NoticePendingReceiveFlag { get; set; }

    public string? NonComplianceFlag { get; set; }

    public string? StabilizationFlag { get; set; }

    public string? CallMarketFlag { get; set; }

    public string? CautionFlag { get; set; }

    public decimal? TwelveMonthDividendYield { get; set; }

    public decimal? PegRatio { get; set; }
}
