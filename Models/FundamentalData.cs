namespace SimpleMcpServer.Api.Models
{
    public class FundamentalData
    {
        public string Symbol { get; set; } = string.Empty;

        public decimal Revenue { get; set; }

        public decimal NetProfit { get; set; }

        public decimal PE { get; set; }

        public decimal ROE { get; set; }

        public decimal EPS { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
