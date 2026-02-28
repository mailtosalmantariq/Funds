using Microsoft.AspNetCore.Mvc;
using ST.Funds.Application.DTO;
using ST.Funds.Application.Services.FundIngestion;
using ST.Funds.Data.Models;

namespace ST.Funds.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundsController : ControllerBase
    {
        private readonly IFundIngestionService _service;
        private readonly ILogger<FundsController> _logger;

        public FundsController(
            IFundIngestionService service,
            ILogger<FundsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ==========================================
        // GET: api/funds
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FundDto>>> GetFunds(
            [FromQuery] FundQueryParameters query,
            CancellationToken cancellationToken)
        {
            try
            {
                var funds = await _service.GetFundsAsync(query);
                return Ok(funds);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetFunds request was cancelled.");
                return StatusCode(499); // Client Closed Request (optional)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving funds.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ==========================================
        // GET: api/funds/{marketCode}
        // ==========================================
        [HttpGet("{marketCode}")]
        public async Task<ActionResult<FundDto>> GetByMarketCode(
            string marketCode,
            CancellationToken cancellationToken)
        {
            try
            {
                var fund = await _service.GetByMarketCodeAsync(marketCode);

                if (fund is null)
                {
                    _logger.LogInformation(
                        "Fund not found for MarketCode {MarketCode}",
                        marketCode);

                    return NotFound();
                }

                return Ok(fund);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "GetByMarketCode request cancelled for {MarketCode}",
                    marketCode);

                return StatusCode(499);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving fund {MarketCode}",
                    marketCode);

                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ==========================================
        // POST: api/funds/refresh
        // ==========================================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            CancellationToken cancellationToken)
        {
            try
            {
                await _service.RefreshFundsAsync(cancellationToken);
                return NoContent();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Fund refresh was cancelled.");
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during fund refresh.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}