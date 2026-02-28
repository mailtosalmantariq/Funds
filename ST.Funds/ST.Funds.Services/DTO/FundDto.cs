

namespace ST.Funds.Application.DTO
{
    public class FundDto
    {
        public string Name { get; set; } = string.Empty;

        public string MarketCode { get; set; } = string.Empty;

        public decimal LastPrice { get; set; }

        public DateTime LastPriceDate { get; set; }

        public decimal OngoingCharge { get; set; }

        public string SectorName { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public string Objective { get; set; } = string.Empty;

        public int? AnalystRating { get; set; }

        public string AnalystRatingLabel { get; set; } = string.Empty;

        public IEnumerable<DocumentDto> Documents { get; set; } = new List<DocumentDto>();

        public IEnumerable<AssetAllocationDto> AssetAllocations { get; set; } = [];

        public IEnumerable<HoldingDto> TopHoldings { get; set; } = [];
    }

}
