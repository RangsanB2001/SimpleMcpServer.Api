using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class SecurityMasterTools
{
    private readonly ISecurityMasterProvider _provider;

    public SecurityMasterTools(ISecurityMasterProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "security_master")]
    [Description(
        "Get the canonical security master record for a Thai SET/mai stock from the Compsec table. " +
        "Returns sec_id, security type (S=Common, P=Preferred, U=Unit Trust, L=ETF, W=Warrant, V=DW, X=DR), " +
        "market type (A=SET, S=mai, O=Options, B=Bond), industry/sector codes, share counts " +
        "(authorized/paid-up/issued/listed/reserved), par value, IPO price, currency, " +
        "listing date, trading date, fiscal year end, listing status (L=Listed, U=Unlisted, D=Delisted, E=Expired), " +
        "and for warrants/DWs: conversion price, ratio, last exercise date.")]
    public async Task<object> GetSecurityMaster(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        CancellationToken cancellationToken = default)
    {
        var normalized = SymbolHelpers.Normalize(symbol);

        var master = await _provider.GetBySymbolAsync(normalized, cancellationToken);

        if (master is null)
        {
            throw new InvalidOperationException($"No security master record found for symbol '{normalized}'");
        }

        return new
        {
            symbol = normalized,
            master,
            decoded = new
            {
                securityType = PsimsLabels.SecType(master.SecType),
                marketType = PsimsLabels.MarketType(master.MkType),
                listingStatus = PsimsLabels.SecListStatus(master.SecListStatus),
                accountForm = PsimsLabels.AccountForm(master.AccountForm)
            }
        };
    }
}
