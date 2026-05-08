namespace SimpleMcpServer.Domain.Entities;

public class FreeFloat
{
    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public DateTime ShareholderAsOfDate { get; set; }

    public decimal? MinorShareholders { get; set; }

    public decimal? PerFreeFloat { get; set; }

    public string? BookCloseType { get; set; }
}
