namespace SimpleMcpServer.Domain.Entities;

public class ShortPosition
{
    public int SecId { get; set; }

    public string SecName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public long? ShortPosOutsVolume { get; set; }
}
