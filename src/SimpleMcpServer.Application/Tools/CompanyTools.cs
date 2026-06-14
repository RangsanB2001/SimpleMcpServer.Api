using ModelContextProtocol.Server;
using SimpleMcpServer.Application.Abstractions;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Tools;

[McpServerToolType]
public class CompanyTools
{
    private readonly ICompanyProvider _provider;

    public CompanyTools(ICompanyProvider provider)
    {
        _provider = provider;
    }

    [McpServerTool(Name = "company_profile")]
    [Description("Get company profile for a Thai SET/mai stock symbol. Joins the Company table with the business table " +
        "to return name (Thai/English), address, contact details, listing status, business description, " +
        "CG/CAC/ESG ratings (from Comprating), dividend policy, and DW issuer credit rating. " +
        "Returns a 'profile' object plus a 'decoded' object with human-readable labels for codes.")]
    public async Task<object> GetCompanyProfile([Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,CancellationToken cancellationToken = default)
    {
        var normalized = SymbolHelpers.Normalize(symbol);

        var profile = await _provider.GetBySymbolAsync(normalized, cancellationToken);

        if (profile is null)
        {
            throw new InvalidOperationException($"No company profile found for symbol '{normalized}'");
        }

        return new
        {
            symbol = normalized,
            profile,
            decoded = new
            {
                companyType = PsimsLabels.CompanyType(profile.ComType),
                issuerRatingOutlook = PsimsLabels.RatingOutlook(profile.IssuerGuarantorRatingOutlook),
                cacCertified = string.Equals(profile.CacFlag, "Y", StringComparison.OrdinalIgnoreCase)
            }
        };
    }
}
