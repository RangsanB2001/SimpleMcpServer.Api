namespace SimpleMcpServer.Domain.Entities;

public class InvestorBreakdown
{
    public DateTime Date { get; set; }

    public string MkType { get; set; } = string.Empty;

    public int IndustryNo { get; set; }

    public int SectorNo { get; set; }

    public int SubsectorNo { get; set; }

    public long? BuyTranCust { get; set; }

    public long? SellTranCust { get; set; }

    public decimal? BuyVolCust { get; set; }

    public decimal? SellVolCust { get; set; }

    public decimal? BuyValCust { get; set; }

    public decimal? SellValCust { get; set; }

    public long? BuyTranInsti { get; set; }

    public long? SellTranInsti { get; set; }

    public decimal? BuyVolInsti { get; set; }

    public decimal? SellVolInsti { get; set; }

    public decimal? BuyValInsti { get; set; }

    public decimal? SellValInsti { get; set; }

    public long? BuyTranForeigner { get; set; }

    public long? SellTranForeigner { get; set; }

    public decimal? BuyVolForeigner { get; set; }

    public decimal? SellVolForeigner { get; set; }

    public decimal? BuyValForeigner { get; set; }

    public decimal? SellValForeigner { get; set; }

    public long? BuyTranBroker { get; set; }

    public long? SellTranBroker { get; set; }

    public decimal? BuyVolBroker { get; set; }

    public decimal? SellVolBroker { get; set; }

    public decimal? BuyValBroker { get; set; }

    public decimal? SellValBroker { get; set; }
}
