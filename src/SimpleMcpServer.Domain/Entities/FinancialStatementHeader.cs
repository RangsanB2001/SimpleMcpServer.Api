namespace SimpleMcpServer.Domain.Entities;

public class FinancialStatementHeader
{
    public string SecName { get; set; } = string.Empty;

    public int SecId { get; set; }

    public string FinStateType { get; set; } = string.Empty;

    public int Fiscal { get; set; }

    public string Quarter { get; set; } = string.Empty;

    public string AdjustFinState { get; set; } = string.Empty;

    public DateTime DateAsOf { get; set; }

    public string? AccountForm { get; set; }

    public string? CurrencyFinState { get; set; }

    public DateTime? StartDateFiscalYear { get; set; }

    public DateTime? StartDateFinState { get; set; }

    public string? StatementFinPositionFlag { get; set; }

    public string? IncomeStatementFlag { get; set; }

    public string? CashFlowFlag { get; set; }

    public string? StatusFinState3m { get; set; }

    public string? StatusFinStateAcc { get; set; }

    public string? StatusStatementFinancialPosition { get; set; }

    public string? StatusStatementCashflow { get; set; }

    public DateTime? DateIncomeStatement { get; set; }

    public DateTime? DateStatementFinancialPosition { get; set; }

    public DateTime? DateStatementCashflow { get; set; }

    public DateTime? StartdateStatementCashflow { get; set; }

    public string? NewsFilenameTh { get; set; }

    public string? NewsFilenameEn { get; set; }
}
