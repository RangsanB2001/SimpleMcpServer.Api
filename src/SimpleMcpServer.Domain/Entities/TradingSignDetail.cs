namespace SimpleMcpServer.Domain.Entities;

public class TradingSignDetail
{
    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public string Sign { get; set; } = string.Empty;

    public DateTime SignPosDate { get; set; }

    public int SignPosReasonSeqNo { get; set; }

    public string? SignPosReason { get; set; }

    public DateTime? ReasonStartDate { get; set; }

    public DateTime? ReasonLiftDate { get; set; }

    public string? ReasonDetailQuarterOfFinancialStatement { get; set; }

    public DateTime? ReasonDetailAsOfDateOfFinancialStatement { get; set; }

    public string? ReasonDetailDescriptionTh { get; set; }

    public string? ReasonDetailDescriptionEn { get; set; }
}
