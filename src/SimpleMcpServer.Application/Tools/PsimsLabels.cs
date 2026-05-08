namespace SimpleMcpServer.Application.Tools;

internal static class PsimsLabels
{
    public static string? MarketType(string? code) => code switch
    {
        "A" => "SET",
        "S" => "mai",
        "O" => "Options",
        "B" => "Bond (TDBC)",
        "C" => "TDBC",
        null or "" => null,
        _ => code
    };

    public static string? SecType(string? code) => code switch
    {
        "S" => "Common Stock",
        "P" => "Preferred Stock",
        "U" => "Unit Trust",
        "L" => "ETF",
        "W" => "Warrant",
        "V" => "Derivative Warrant (DW)",
        "X" => "Depositary Receipt (DR)",
        "F" => "Foreign Stock",
        "R" => "NVDR",
        "C" => "Convertible",
        "D" => "Debenture",
        "T" => "Thai Trust Fund",
        null or "" => null,
        _ => code
    };

    public static string? CompanyType(string? code) => code switch
    {
        "L" => "Listed",
        "U" => "Unlisted",
        "D" => "Delisted",
        "I" => "Issuer",
        "R" => "Regulator",
        "M" => "Risk Manager",
        null or "" => null,
        _ => code
    };

    public static string? SecListStatus(string? code) => code switch
    {
        "L" => "Listed on SET",
        "U" => "Unlisted",
        "D" => "Delisted",
        "E" => "Expired",
        "C" => "Cancelled",
        null or "" => null,
        _ => code
    };

    public static string? Sign(string? code) => code switch
    {
        "NP" => "Notice Pending",
        "NR" => "Notice Received",
        "SP" => "Suspension",
        "H"  => "Halt",
        "C"  => "Caution",
        "CB" => "Caution (Business)",
        "CS" => "Caution (Financial)",
        "CG" => "Caution (Governance)",
        "CF" => "Caution (Free Float)",
        "CM" => "Call Market",
        "ST" => "Stabilization",
        "NC" => "Non-Compliance",
        null or "" => null,
        _ => code
    };

    public static string? Benefit(string? code) => code switch
    {
        "XD" => "Ex-Dividend",
        "XR" => "Ex-Right",
        "XI" => "Ex-Interest",
        "XE" => "Ex-Exercise",
        "XM" => "Ex-Meeting",
        "XB" => "Ex-Bond",
        "XN" => "Ex-Capital Return",
        "XW" => "Ex-Warrant",
        "XS" => "Ex-Short Term Warrant",
        "XT" => "Ex-Transferable Subscription Right",
        "XA" => "Ex-All",
        null or "" => null,
        _ => code
    };

    public static string? DividendType(string? code) => code switch
    {
        "CD" => "Cash Dividend",
        "SD" => "Stock Dividend",
        null or "" => null,
        _ => code
    };

    public static string? DividendFlag(string? code) => code switch
    {
        "" or null => null,
        "@" => "Special Dividend (period specified)",
        "$" => "Special Dividend (with period)",
        "&" => "Special Dividend (no period)",
        _ => code
    };

    public static string? SourceOfDividendPayment(string? code) => code switch
    {
        "1" => "Net Profit",
        "2" => "Retained Earnings",
        "3" => "Net Profit and Retained Earnings",
        null or "" => null,
        _ => code
    };

    public static string? FinStateType(string? code) => code switch
    {
        "C" => "Consolidated",
        "E" => "Equity Method",
        "U" => "Separate (Single Entity)",
        null or "" => null,
        _ => code
    };

    public static string? AdjustFinState(string? code) => code switch
    {
        "O" => "Original",
        "R" => "Restatement",
        "P" => "Pro Forma",
        null or "" => null,
        _ => code
    };

    public static string? AccountForm(string? code) => code switch
    {
        "1" => "Bank",
        "2" => "Other Finance",
        "3" => "Securities",
        "4" => "Insurance",
        "5" => "Fund",
        "6" => "Other",
        null or "" => null,
        _ => code
    };

    public static string? Quarter(string? code) => code switch
    {
        "1" => "Q1",
        "2" => "Q2",
        "3" => "Q3",
        "9" => "Annual",
        null or "" => null,
        _ => code
    };

    public static string? TradingMethod(string? code) => code switch
    {
        "A" => "Auto Matching",
        "T" => "Trade Report",
        "O" => "Odd Lot",
        null or "" => null,
        _ => code
    };

    public static string? TradeReportSubType(string? code) => code switch
    {
        "F" => "Trade Report - Foreign",
        "B" => "Trade Report - Big Lot",
        "N" => "Trade Report - Buy-in",
        null or "" => null,
        _ => code
    };

    public static string? FinStatementStatus(string? code) => code switch
    {
        "U" => "Unreviewed",
        "R" => "Reviewed",
        "A" => "Audited",
        "P" => "Pro Forma",
        null or "" => null,
        _ => code
    };

    public static string? ChangeParType(string? code) => code switch
    {
        "S" => "Split",
        "R" => "Reduction",
        null or "" => null,
        _ => code
    };

    public static string? CancelStatus(string? code) => code switch
    {
        "C" => "Cancelled",
        null or "" => null,
        _ => code
    };

    public static string? BookCloseType(string? code) => code switch
    {
        "XM"  => "Meeting",
        "IPO" => "Initial Public Offering",
        "BRS" => "Business Restructure",
        "XD"  => "Dividend",
        "XR"  => "Rights",
        "XW"  => "Warrant",
        "XE"  => "Exercise",
        null or "" => null,
        _ => code
    };

    public static string? CollateralType(string? code) => code switch
    {
        "F" => "Full Covered",
        "P" => "Partial Covered",
        "N" => "Non-Collateralized",
        null or "" => null,
        _ => code
    };

    public static string? OptionsStyle(string? code) => code switch
    {
        "E" => "European",
        "A" => "American",
        null or "" => null,
        _ => code
    };

    public static string? OptionsType(string? code) => code switch
    {
        "C" => "Call",
        "P" => "Put",
        null or "" => null,
        _ => code
    };

    public static string? SettleType(string? code) => code switch
    {
        "C" => "Cash Settlement",
        "S" => "Share Settlement",
        null or "" => null,
        _ => code
    };

    public static string? RatingOutlook(string? code) => code switch
    {
        "P" => "Positive",
        "S" => "Stable",
        "N" => "Negative",
        "D" => "Developing",
        null or "" => null,
        _ => code
    };
}
