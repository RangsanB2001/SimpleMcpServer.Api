namespace SimpleMcpServer.Domain.Constants;

public static class PsimsCodes
{
    public static class MarketType
    {
        public const string Set = "A";

        public const string Mai = "S";

        public const string Options = "O";

        public const string Bond = "B";
    }

    public static class TradingMethod
    {
        public const string AutoMatching = "A";

        public const string TradeReport = "T";

        public const string OddLot = "O";
    }

    public static class TradeReportSubType
    {
        public const string Foreign = "F";

        public const string Biglot = "B";

        public const string BuyIn = "N";
    }

    public static class FinStateType
    {
        public const string Consolidated = "C";

        public const string EquityMethod = "E";

        public const string Separate = "U";
    }

    public static class AdjustFinState
    {
        public const string Original = "O";

        public const string Restatement = "R";

        public const string ProForma = "P";
    }

    public static class Quarter
    {
        public const string Q1 = "1";

        public const string Q2 = "2";

        public const string Q3 = "3";

        public const string Annual = "9";

        public static bool IsValid(string quarter) =>
            quarter is Q1 or Q2 or Q3 or Annual;
    }

    public static class CancelStatus
    {
        public const string Cancelled = "C";
    }

    public static class RatingType
    {
        public const string Cg = "CG";

        public const string Cac = "CAC";

        public const string Esg = "ESG";
    }
}
