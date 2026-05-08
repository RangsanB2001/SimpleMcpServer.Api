namespace SimpleMcpServer.Domain.Entities;

public class TradingSign
{
    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public string Sign { get; set; } = string.Empty;

    public DateTime SignPosDate { get; set; }

    public DateTime? SignLiftDate { get; set; }

    public string? SignPosNewsfnameTh { get; set; }

    public string? SignPosNewsfnameEng { get; set; }

    public string? SignLiftNewsfnameTh { get; set; }

    public string? SignLiftNewsfnameEng { get; set; }
}
