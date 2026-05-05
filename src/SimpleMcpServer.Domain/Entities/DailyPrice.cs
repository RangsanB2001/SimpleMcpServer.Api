namespace SimpleMcpServer.Domain.Entities;

public class DailyPrice
{
    public DateTime Date { get; set; }

    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public string MkType { get; set; } = string.Empty;

    public string TradingMethod { get; set; } = string.Empty;

    public decimal? Prior { get; set; }

    public decimal? Open { get; set; }

    public decimal? High { get; set; }

    public decimal? Low { get; set; }

    public decimal? Close { get; set; }

    public decimal? Volume { get; set; }

    public decimal? Value { get; set; }

    public decimal? AvgPrice { get; set; }
}
