namespace SimpleMcpServer.Domain.Entities;

public class NvdrHolding
{
    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public DateTime DateAsOf { get; set; }

    public DateTime? DateData { get; set; }

    public decimal? NoShareInhand { get; set; }

    public decimal? AvgCost { get; set; }

    public decimal? PVolume { get; set; }

    public decimal? PValue { get; set; }
}
