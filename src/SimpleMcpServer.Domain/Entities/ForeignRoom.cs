namespace SimpleMcpServer.Domain.Entities;

public class ForeignRoom
{
    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public DateTime DateAsOf { get; set; }

    public decimal? PForeignLimit { get; set; }

    public decimal? PSpforeignLimit { get; set; }

    public decimal? TotShare { get; set; }

    public decimal? TotForeignshares { get; set; }

    public decimal? ForeignRoomShares { get; set; }

    public decimal? ForeignQueueShares { get; set; }

    public decimal? NoShareAvailable { get; set; }

    public decimal? TotHoldshares { get; set; }

    public decimal? PercentForeignQueue { get; set; }

    public decimal? PercentForeignAvailable { get; set; }

    public string? SpforeignLimitFlag { get; set; }
}
