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
            var funds = await _service.GetFundsAsync(query);
            return Ok(funds);
        }

        // ==========================================
        // GET: api/funds/{marketCode}
        // ==========================================
        [HttpGet("{marketCode}")]
        public async Task<ActionResult<FundDto>> GetByMarketCode(
            string marketCode,
            CancellationToken cancellationToken)
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

        // ==========================================
        // POST: api/funds/refresh
        // ==========================================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            CancellationToken cancellationToken)
        {
            await _service.RefreshFundsAsync(cancellationToken);

            return NoContent();
        }
    }
}