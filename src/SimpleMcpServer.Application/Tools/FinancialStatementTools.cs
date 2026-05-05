using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
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
        "Defaults to consolidated original statements (fin_state_type=C, adjust_fin_state=O).")]
    public async Task<object> GetFinancialStatements(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("Fiscal year, e.g. 2024.")] int fiscal,
        [Description("Quarter: '1', '2', '3' for quarterly, '9' for annual.")] string quarter,
        [Description("Statement type: 'C' = Consolidated (default), 'E' = Equity method, 'U' = Separate.")] string finStateType = "C",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("symbol must not be empty", nameof(symbol));
        }

        if (quarter is not ("1" or "2" or "3" or "9"))
        {
            throw new ArgumentException("quarter must be one of '1', '2', '3', '9'", nameof(quarter));
        }

        if (finStateType is not ("C" or "E" or "U"))
        {
            throw new ArgumentException("finStateType must be one of 'C', 'E', 'U'", nameof(finStateType));
        }

        var rows = (await _provider.GetByPeriodAsync(symbol, fiscal, quarter, finStateType, cancellationToken)).ToArray();

        if (rows.Length == 0)
        {
            throw new InvalidOperationException(
                $"No financial statement lines found for symbol '{symbol}' (fiscal {fiscal}, quarter {quarter}, type {finStateType})");
        }

        return new
        {
            symbol,
            fiscal,
            quarter,
            finStateType,
            count = rows.Length,
            rows
        };
    }
}
