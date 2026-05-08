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

    public string? Url { get; set; }

    public string? EstablishDate { get; set; }

    public string? ComType { get; set; }

    public string? BusinessTypeTh { get; set; }

    public string? BusinessTypeEn { get; set; }

    public string? CgScore { get; set; }

    public string? CgRatingFlag { get; set; }

    public DateTime? CgAsOf { get; set; }

    public string? CacFlag { get; set; }

    public DateTime? CacAsOf { get; set; }

    public string? EsgRating { get; set; }

    public DateTime? EsgAsOf { get; set; }

    public string? DividePolTh { get; set; }

    public string? DividePolEng { get; set; }

    public string? DividendPolicyFileNameTh { get; set; }

    public string? DividendPolicyFileNameEn { get; set; }

    public string? JuristicPersonRegistration { get; set; }

    public string? IssuerGuarantorCreditRating { get; set; }

    public string? IssuerGuarantorRatingOutlook { get; set; }

    public string? CreditRatingAgencyTh { get; set; }

    public string? CreditRatingAgencyEng { get; set; }

    public DateTime? DateAsOf { get; set; }
}
