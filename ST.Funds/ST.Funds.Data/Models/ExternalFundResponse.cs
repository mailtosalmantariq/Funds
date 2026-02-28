
namespace ST.Funds.Data.Models
{
    public class ExternalFundResponse
    {
        public ExternalData? data { get; set; }
    }

    public class ExternalData
    {
        public ExternalQuote? quote { get; set; }
        public ExternalProfile? profile { get; set; }
        public ExternalRatings? ratings { get; set; }
        public List<ExternalDocument>? documents { get; set; }
        public ExternalPortfolio? portfolio { get; set; }
    }

    public class ExternalQuote
    {
        public string name { get; set; } = string.Empty;
        public string marketCode { get; set; } = string.Empty;
        public decimal lastPrice { get; set; }
        public DateTime lastPriceDate { get; set; }
        public decimal ongoingCharge { get; set; }
        public string sectorName { get; set; } = string.Empty;
        public string currency { get; set; } = string.Empty;
    }

    public class ExternalProfile
    {
        public string objective { get; set; } = string.Empty;
    }

    public class ExternalRatings
    {
        public int? analystRating { get; set; }
        public string? analystRatingLabel { get; set; }
    }

    public class ExternalDocument
    {
        public string type { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
    }

    public class ExternalPortfolio
    {
        public List<ExternalAsset>? asset { get; set; }
        public List<ExternalHolding>? top10Holdings { get; set; }
    }

    public class ExternalAsset
    {
        public string label { get; set; } = string.Empty;
        public decimal value { get; set; }
    }

    public class ExternalHolding
    {
        public string name { get; set; } = string.Empty;
        public decimal weighting { get; set; }
    }
}
