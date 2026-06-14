namespace SimpleMcpServer.Domain.Entities;

public class ParChange
{
    public string SecName { get; set; } = string.Empty;

    public DateTime NewsAnnDate { get; set; }

    public int SeqOfAnn { get; set; }

    public DateTime? BoardDate { get; set; }

    public decimal? OldParVal { get; set; }

    public decimal? NewParVal { get; set; }

    public DateTime? EffectDate { get; set; }

    public string? ChangeParType { get; set; }

    public string? CancelStatus { get; set; }
}
