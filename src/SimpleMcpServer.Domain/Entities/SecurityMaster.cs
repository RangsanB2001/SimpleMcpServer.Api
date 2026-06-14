namespace SimpleMcpServer.Domain.Entities;

public class SecurityMaster
{
    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public int? ComId { get; set; }

    public string? SecFullnameTh { get; set; }

    public string? SecFullnameEng { get; set; }

    public string? SecType { get; set; }

    public string? MkType { get; set; }

    public int? IndustryNo { get; set; }

    public int? SectorNo { get; set; }

    public int? SubsectorNo { get; set; }

    public decimal? NoAuthorShares { get; set; }

    public decimal? NoPaidupShares { get; set; }

    public decimal? NoIssuedShares { get; set; }

    public decimal? NoListedShares { get; set; }

    public decimal? IpoPrice { get; set; }

    public string? Currency { get; set; }

    public decimal? ParVal { get; set; }

    public DateTime? ListDate { get; set; }

    public DateTime? TradingDate { get; set; }

    public DateTime? IssuedDate { get; set; }

    public DateTime? ExpiredDate { get; set; }

    public string? AccountForm { get; set; }

    public string? FiscalYearEnddate { get; set; }

    public string? SecListStatus { get; set; }

    public DateTime? DelistDate { get; set; }

    public string? CauseDel { get; set; }

    public DateTime? LastTradingDate { get; set; }

    public DateTime? LastExercise { get; set; }

    public decimal? ConverExStrPrice { get; set; }

    public string? ConverExRatio { get; set; }
}
