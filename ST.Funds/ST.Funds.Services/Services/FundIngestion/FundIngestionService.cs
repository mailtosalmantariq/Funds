using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ST.Funds.Application.Config;
using ST.Funds.Application.DTO;
using ST.Funds.Data.DataContext;
using ST.Funds.Data.Models;

namespace ST.Funds.Application.Services.FundIngestion
{
    public class FundIngestionService : IFundIngestionService
    {
        private readonly HttpClient _httpClient;
        private readonly FundsDbContext _db;
        private readonly List<FundSourceConfig> _sources;
        private readonly ILogger<FundIngestionService> _logger;

        public FundIngestionService(
            HttpClient httpClient,
            FundsDbContext db,
            IOptions<List<FundSourceConfig>> options,
            ILogger<FundIngestionService> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _sources = options.Value;
            _logger = logger;
        }

        // =====================================================
        // GET ALL FUNDS
        // =====================================================
        public async Task<IEnumerable<FundDto>> GetFundsAsync(FundQueryParameters query)
        {
            var dbQuery = BuildFundQuery(query);
            return await ProjectToDto(dbQuery).ToListAsync();
        }

        // =====================================================
        // GET BY MARKET CODE
        // =====================================================
        public async Task<FundDto?> GetByMarketCodeAsync(string marketCode)
        {
            var query = BaseFundQuery()
                .Where(f => f.MarketCode == marketCode);

            return await ProjectToDto(query).FirstOrDefaultAsync();
        }

        // =====================================================
        // REFRESH FUNDS
        // =====================================================
        public async Task RefreshFundsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting fund ingestion.");

            foreach (var source in _sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ProcessSourceAsync(source, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Fund ingestion completed.");
        }

        // =====================================================
        // PRIVATE HELPERS
        // =====================================================

        private IQueryable<Fund> BaseFundQuery()
        {
            return _db.Funds
                .AsNoTracking()
                .Include(f => f.Documents)
                .Include(f => f.AssetAllocations)
                .Include(f => f.TopHoldings);
        }

        private IQueryable<Fund> BuildFundQuery(FundQueryParameters query)
        {
            var dbQuery = BaseFundQuery();

            if (query.MaxOngoingCharge.HasValue)
                dbQuery = dbQuery.Where(f =>
                    f.OngoingCharge <= query.MaxOngoingCharge.Value);

            if (query.AnalystRating.HasValue)
                dbQuery = dbQuery.Where(f =>
                    f.AnalystRating == query.AnalystRating.Value);

            if (!string.IsNullOrWhiteSpace(query.SectorName))
                dbQuery = dbQuery.Where(f =>
                    f.SectorName == query.SectorName);

            return dbQuery;
        }

        private IQueryable<FundDto> ProjectToDto(IQueryable<Fund> query)
        {
            return query.Select(f => new FundDto
            {
                Name = f.Name,
                MarketCode = f.MarketCode,
                LastPrice = f.LastPrice,
                LastPriceDate = f.LastPriceDate,
                OngoingCharge = f.OngoingCharge,
                SectorName = f.SectorName,
                Currency = f.Currency,
                Objective = f.Objective,
                AnalystRating = f.AnalystRating,
                AnalystRatingLabel = f.AnalystRatingLabel ?? string.Empty,

                Documents = f.Documents.Select(d => new DocumentDto
                {
                    Type = d.Type,
                    Url = d.Url
                }),

                AssetAllocations = f.AssetAllocations.Select(a => new AssetAllocationDto
                {
                    Label = a.Label,
                    Value = a.Value
                }),

                TopHoldings = f.TopHoldings.Select(h => new HoldingDto
                {
                    Name = h.Name,
                    Weighting = h.Weighting
                })
            });
        }

        private async Task ProcessSourceAsync(
            FundSourceConfig source,
            CancellationToken cancellationToken)
        {
            try
            {
                var external = await _httpClient
                    .GetFromJsonAsync<ExternalFundResponse>(
                        source.Url,
                        cancellationToken);

                if (external?.data?.quote == null)
                {
                    _logger.LogWarning("Invalid response from {Url}", source.Url);
                    return;
                }

                var fund = await GetOrCreateFundAsync(
                    external.data.quote.marketCode,
                    cancellationToken);

                UpdateFundProperties(fund, external);
                UpdateFundCollections(fund, external);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing source {Url}",
                    source.Url);
            }
        }

        private async Task<Fund> GetOrCreateFundAsync(
            string marketCode,
            CancellationToken cancellationToken)
        {
            var fund = await _db.Funds
                .Include(f => f.Documents)
                .Include(f => f.AssetAllocations)
                .Include(f => f.TopHoldings)
                .FirstOrDefaultAsync(
                    f => f.MarketCode == marketCode,
                    cancellationToken);

            if (fund == null)
            {
                fund = new Fund();
                await _db.Funds.AddAsync(fund, cancellationToken);
            }
            else
            {
                _db.Documents.RemoveRange(fund.Documents);
                _db.AssetAllocations.RemoveRange(fund.AssetAllocations);
                _db.Holdings.RemoveRange(fund.TopHoldings);
            }

            return fund;
        }

        private void UpdateFundProperties(Fund fund, ExternalFundResponse external)
        {
            var quote = external.data!.quote!;

            fund.Name = quote.name;
            fund.MarketCode = quote.marketCode;
            fund.LastPrice = quote.lastPrice;
            fund.LastPriceDate = quote.lastPriceDate;
            fund.OngoingCharge = quote.ongoingCharge;
            fund.SectorName = quote.sectorName;
            fund.Currency = quote.currency;
            fund.Objective = external.data.profile?.objective ?? string.Empty;
            fund.AnalystRating = external.data.ratings?.analystRating;
            fund.AnalystRatingLabel =
                external.data.ratings?.analystRatingLabel ?? string.Empty;
        }

        private void UpdateFundCollections(Fund fund, ExternalFundResponse external)
        {
            fund.Documents = external.data?.documents?
                .Select(d => new FundDocument
                {
                    Type = d.type,
                    Url = d.url
                }).ToList() ?? new List<FundDocument>();

            fund.AssetAllocations = external.data?.portfolio?.asset?
                .Select(a => new AssetAllocation
                {
                    Label = a.label,
                    Value = a.value
                }).ToList() ?? new List<AssetAllocation>();

            fund.TopHoldings = external.data?.portfolio?.top10Holdings?
                .Select(h => new Holding
                {
                    Name = h.name,
                    Weighting = h.weighting
                }).ToList() ?? new List<Holding>();
        }
    }
}