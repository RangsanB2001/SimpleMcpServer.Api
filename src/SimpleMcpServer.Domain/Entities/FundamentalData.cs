namespace SimpleMcpServer.Domain.Entities;

public class FundamentalData
{
    public int SecId { get; set; }

    public string SecName { get; set; } = string.Empty;

    public int Fiscal { get; set; }

    public string Quarter { get; set; } = string.Empty;

    public DateTime FsPeriod { get; set; }

    public decimal? LastPrice { get; set; }

    public decimal? Pe { get; set; }

    public decimal? Pbv { get; set; }

    public decimal? MkCap { get; set; }

    public decimal? ListedShare { get; set; }

    public decimal? EarningPerShare { get; set; }

    public decimal? BookNetValue { get; set; }

    public decimal? DividePerShare { get; set; }

    public decimal? DividendYield { get; set; }

    public decimal? NetProfitCons { get; set; }

    public decimal? Roe { get; set; }

    public decimal? Roa { get; set; }

    public decimal? RoeLast4q { get; set; }

    public decimal? RoaLast4q { get; set; }

    public decimal? TotalAsset { get; set; }

    public decimal? TotalEquity { get; set; }

    public decimal? Liabilities { get; set; }

    public decimal? Cash { get; set; }

    public decimal? DebtEquityRatio { get; set; }

    public decimal? Gearing { get; set; }

    public DateTime Timestamp { get; set; }
}
