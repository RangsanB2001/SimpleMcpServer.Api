namespace SimpleMcpServer.Domain.Entities;

public class FinancialStatementLine
{
    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public int Fiscal { get; set; }

    public string Quarter { get; set; } = string.Empty;

    public string FinStateType { get; set; } = string.Empty;

    public string AdjustFinState { get; set; } = string.Empty;

    public DateTime DateAsOf { get; set; }

    public string AccountId { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public decimal? AccuAmount { get; set; }
}
