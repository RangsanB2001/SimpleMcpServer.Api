using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using SimpleMcpServer.Domain.Constants;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class FinancialStatementTools
{
    private readonly IFinancialStatementProvider _provider;

    public FinancialStatementTools(IFinancialStatementProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "financial_statements")]
    [Description(
        "Get raw financial statement line items (P&L, balance sheet, cash flow) for a Thai SET/mai stock " +
        "from the FinStmtDet table for a given fiscal year and quarter. " +
        "Each row carries an SET accountid (e.g. 460100 = net profit attributable to parent) with amount and accumulated amount. " +
        "Defaults to consolidated original statements (fin_state_type=C, adjust_fin_state=O). " +
        "Pass adjustFinState='R' to get restated figures (often more accurate after a company's later corrections).")]
    public async Task<object> GetFinancialStatements(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("Fiscal year, e.g. 2024.")] int fiscal,
        [Description("Quarter: '1', '2', '3' for quarterly, '9' for annual.")] string quarter,
        [Description("Statement type: 'C' = Consolidated (default), 'E' = Equity method, 'U' = Separate.")] string finStateType = PsimsCodes.FinStateType.Consolidated,
        [Description("Adjustment status: 'O' = Original (default), 'R' = Restatement, 'P' = Pro forma.")] string adjustFinState = PsimsCodes.AdjustFinState.Original,
        CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = SymbolHelpers.Normalize(symbol);

        if (!PsimsCodes.Quarter.IsValid(quarter))
        {
            throw new ArgumentException("quarter must be one of '1', '2', '3', '9'", nameof(quarter));
        }

        if (finStateType is not (PsimsCodes.FinStateType.Consolidated or PsimsCodes.FinStateType.EquityMethod or PsimsCodes.FinStateType.Separate))
        {
            throw new ArgumentException("finStateType must be one of 'C', 'E', 'U'", nameof(finStateType));
        }

        if (adjustFinState is not (PsimsCodes.AdjustFinState.Original or PsimsCodes.AdjustFinState.Restatement or PsimsCodes.AdjustFinState.ProForma))
        {
            throw new ArgumentException("adjustFinState must be one of 'O', 'R', 'P'", nameof(adjustFinState));
        }

        var headerTask = _provider.GetHeaderByPeriodAsync(normalizedSymbol, fiscal, quarter, finStateType, adjustFinState, cancellationToken);
        var rowsTask = _provider.GetByPeriodAsync(normalizedSymbol, fiscal, quarter, finStateType, adjustFinState, cancellationToken);

        await Task.WhenAll(headerTask, rowsTask);

        var header = await headerTask;
        var rows = (await rowsTask).ToArray();

        if (rows.Length == 0)
        {
            throw new InvalidOperationException(
                $"No financial statement lines found for symbol '{normalizedSymbol}' (fiscal {fiscal}, quarter {quarter}, type {finStateType}, adjust {adjustFinState})");
        }

        return new
        {
            symbol = normalizedSymbol,
            fiscal,
            quarter,
            finStateType,
            adjustFinState,
            header,
            decoded = new
            {
                quarter = PsimsLabels.Quarter(quarter),
                finStateType = PsimsLabels.FinStateType(finStateType),
                adjustFinState = PsimsLabels.AdjustFinState(adjustFinState),
                accountForm = PsimsLabels.AccountForm(header?.AccountForm),
                statusFinState3m = PsimsLabels.FinStatementStatus(header?.StatusFinState3m),
                statusFinStateAcc = PsimsLabels.FinStatementStatus(header?.StatusFinStateAcc),
                statusStatementFinancialPosition = PsimsLabels.FinStatementStatus(header?.StatusStatementFinancialPosition),
                statusStatementCashflow = PsimsLabels.FinStatementStatus(header?.StatusStatementCashflow)
            },
            count = rows.Length,
            rows
        };
    }
}
