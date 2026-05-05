namespace SimpleMcpServer.Domain.Entities;

public class CompanyProfile
{
    public string SecName { get; set; } = string.Empty;

    public int ComId { get; set; }

    public string? ComNameTh { get; set; }

    public string? ComNameEng { get; set; }

    public string? ComAddrTh { get; set; }

    public string? ComAddrEng { get; set; }

    public string? Zipcode { get; set; }

    public string? TelNum { get; set; }

    public string? FaxNum { get; set; }

    public string? Email { get; set; }

    public string? Url { get; set; }

    public string? EstablishDate { get; set; }

    public string? ComType { get; set; }

    public string? BusinessTypeTh { get; set; }

    public string? BusinessTypeEn { get; set; }

    public string? CgScore { get; set; }

    public string? CacFlag { get; set; }

    public string? DividePolTh { get; set; }

    public string? DividePolEng { get; set; }
}
