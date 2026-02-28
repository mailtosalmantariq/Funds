
namespace ST.Funds.Data.Models
{
    public class FundDocument
    {
        public int Id { get; set; }

        public string ExternalDocumentId { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public int FundId { get; set; }

        public Fund Fund { get; set; } = null!;
    }
}
