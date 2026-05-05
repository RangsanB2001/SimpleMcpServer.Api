namespace SimpleMcpServer.Domain.Entities;

public class Dividend
{
    public string SecName { get; set; } = string.Empty;

    public DateTime NewsAnnDate { get; set; }

    public int SeqOfAnn { get; set; }

    public DateTime? BoardDate { get; set; }

    public DateTime? BookClosingDate { get; set; }

    public DateTime? RecordDate { get; set; }

    public DateTime? BeginXDate { get; set; }

    public DateTime? EndingXDate { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? DividendType { get; set; }

    public string? DividendFlag { get; set; }

    public decimal? DividendPrice { get; set; }

    public string? ShareDividendRatio { get; set; }

    public decimal? AllotedShares { get; set; }

    public decimal? ParValue { get; set; }

    public string? SourceOfDividendPayment { get; set; }

    public string? CancelStatus { get; set; }
}
