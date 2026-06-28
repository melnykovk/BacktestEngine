using BacktestingEngine.BackgroundServices;
using BacktestingEngine.DTO;
using BacktestingEngine.Data;
using BacktestingEngine.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BacktestingEngine.Services;
using BacktestingEngine.Engine;
using BacktestingEngine.Engine.Grid;
using BacktestingEngine.Engine.Core;

namespace BacktestingEngine.Controllers
{

    [ApiController]
    [Route("api/[controller]")]


    public class BinanceController : ControllerBase
    {
        private readonly IBinanceKlineService _binanceKlineService;
        private readonly ILogger<BinanceController> _logger;
        private readonly IKlinesUploadFromDbService _klinesUploadFromDbService;
        private readonly EngineBuilder _engine;

        public BinanceController(IBinanceKlineService binanceKlineService, ILogger<BinanceController> logger, IKlinesUploadFromDbService klinesUploadFromDbService, EngineBuilder engine)
        {
            _binanceKlineService = binanceKlineService;
            _logger = logger;
            _klinesUploadFromDbService = klinesUploadFromDbService;
            _engine = engine;
        }



        // Example: POST /api/binance/upload-from-db?symbol=BTCUSDT&interval=1h&startDate=2025-01-01&endDate=2025-01-31
        [HttpPost("upload-from-db")]
        public async Task<IActionResult> UploadKlinesFromDb([FromBody] RangeBacktestData rangeData)
        {
            try
            {
                var data = await _klinesUploadFromDbService.GetKlinesAsync(rangeData);

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // // Example: POST /api/binance/engine-check?symbol=BTCUSDT&interval=1h&startDate=2025-01-01&endDate=2025-01-31
        [HttpPost("engine-check")]
        public async Task<IActionResult> CheckEngine([FromBody] RangeBacktestData rangeData)
        {
            try
            {
                await _klinesUploadFromDbService.CheckKlinesOnErrors(rangeData);

                return Ok(new { message = "Engine check completed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // Example: POST /api/binance/grid-check
        [HttpPost("grid-check")]
        public async Task<ActionResult<List<BacktestStats>>> CheckGrid([FromBody] BacktestBatchRequest batchRequest)
        {
            try
            {
                var results = await _engine.RunGridPairAsync(batchRequest);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Example: POST /api/binance/fetch-range
        [HttpPost("binance-klines-upload-range")]
        public async Task<ActionResult<DbUploadKlinesResponseDto>> FetchKlinesRange([FromBody] DbUploadKlinesRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Symbol) || string.IsNullOrWhiteSpace(request.Interval))
            {
                return BadRequest("Symbol and Interval are required parameters.");
            }

            if (request.StartDate > DateTime.UtcNow)
            {
                return BadRequest(new { message = "startDate cannot be in the future." });
            }
            if (request.EndDate.HasValue && request.EndDate.Value > DateTime.UtcNow)
            {
                return BadRequest(new { message = "endDate cannot be in the future." });
            }
            if (request.EndDate.HasValue && request.StartDate >= request.EndDate.Value)
            {
                return BadRequest(new { message = "startDate must be earlier than endDate." });
            }
            try
            {
                var endDateStr = request.EndDate?.ToString("O") ?? "yesterday 23:59:59";
                _logger.LogInformation($"Download request in range : {request.Symbol} {request.Interval} from {request.StartDate:O} to {endDateStr}");

                var response = await _binanceKlineService.FetchAndSaveKlinesFromDateAsync(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("get-db-klines")]
        public async Task<ActionResult<IReadOnlyList<DbCandleSummaryDto>>> Get()
        {
            return Ok(await _klinesUploadFromDbService.GetCandleDatasetsSummaryAsync());
        }

    }
}