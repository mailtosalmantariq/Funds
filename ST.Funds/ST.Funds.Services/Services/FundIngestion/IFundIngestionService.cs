using ST.Funds.Application.DTO;
using ST.Funds.Data.Models;

namespace ST.Funds.Application.Services.FundIngestion
{
    public interface IFundIngestionService
    {
        Task<IEnumerable<FundDto>> GetFundsAsync(FundQueryParameters query);
        Task<FundDto?> GetByMarketCodeAsync(string marketCode);
        Task RefreshFundsAsync(CancellationToken cancellationToken = default);
    }
}
