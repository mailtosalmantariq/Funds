
namespace ST.Funds.Data.Models
{
    public class Fund
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string MarketCode { get; set; } = string.Empty;

        public decimal LastPrice { get; set; }

        public DateTime LastPriceDate { get; set; }

        public decimal OngoingCharge { get; set; }

        public string SectorName { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public string Objective { get; set; } = string.Empty;

        public int? AnalystRating { get; set; }

        public int? SRRI { get; set; }

        public string AnalystRatingLabel { get; set; } = string.Empty;

        public ICollection<FundDocument> Documents { get; set; } = new List<FundDocument>();

        public ICollection<AssetAllocation> AssetAllocations { get; set; } = new List<AssetAllocation>();

        public ICollection<Holding> TopHoldings { get; set; } = new List<Holding>();
    }
}
