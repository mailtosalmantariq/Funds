
namespace ST.Funds.Data.Models
{
    public class AssetAllocation
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;

        public decimal Value { get; set; }

        public int FundId { get; set; }

        public Fund Fund { get; set; } = null!;
    }
}
