
namespace ST.Funds.Data.Models
{
    public class Holding
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Weighting { get; set; }

        public int FundId { get; set; }

        public Fund Fund { get; set; } = null!;
    }
}
