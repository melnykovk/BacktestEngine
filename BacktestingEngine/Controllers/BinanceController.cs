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
        private readonly ICandleRepository _candleRepository;
        private readonly EngineBuilder _engine;
        private readonly IBacktestReusltService _backtestResultService;

        public BinanceController(
            IBinanceKlineService binanceKlineService, 
            ILogger<BinanceController> logger, 
            IKlinesUploadFromDbService klinesUploadFromDbService, 
            ICandleRepository candleRepository, 
            EngineBuilder engine,
            IBacktestReusltService backtestReusltService)
        {
            _binanceKlineService = binanceKlineService;
            _logger = logger;
            _klinesUploadFromDbService = klinesUploadFromDbService;
            _candleRepository = candleRepository;
            _engine = engine;
            _backtestResultService = backtestReusltService;
        }



        // Example: POST /api/binance/upload-from-db?symbol=BTCUSDT&interval=1h&startDate=2025-01-01&endDate=2025-01-31
        [HttpPost("upload-from-db")]
        public async Task<IActionResult> UploadKlinesFromDb([FromBody] RangeBacktestData rangeData, CancellationToken _token)
        {
            try
            {
                var data = await _klinesUploadFromDbService.GetKlinesAsync(rangeData, _token);

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
        public async Task<IActionResult> CheckEngine([FromBody] RangeBacktestData rangeData, CancellationToken _token)
        {
            try
            {
                await _klinesUploadFromDbService.CheckKlinesOnErrors(rangeData, _token);

                return Ok(new { message = "Engine check completed successfully." });
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation cancelled by user.");
                return StatusCode(499, new { message = "Request was cancelled by the client." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // Example: POST /api/binance/grid-check
        [HttpPost("backtest-run")]
        public async Task<ActionResult<BacktestStats>> CheckGrid([FromBody] BacktestBatchRequest batchRequest, CancellationToken _token)
        {
            try
            {
                var results = await _backtestResultService.RunBacktestAndSaveResult(batchRequest, _token);
                return Ok(results);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation cancelled by user.");
                return StatusCode(499, new { message = "Request was cancelled by the client." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Example: POST /api/binance/fetch-range
        [HttpPost("upload-klines-dataset")]
        public async Task<ActionResult<DbUploadKlinesResponseDto>> FetchKlinesRange([FromBody] DbUploadKlinesRequestDto request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Symbol) || string.IsNullOrWhiteSpace(request.Interval))
                return BadRequest("Symbol and Interval are required parameters.");
            if (request.StartDate > DateTime.UtcNow)     
                return BadRequest(new { message = "startDate cannot be in the future." });
            if(request.StartDate == default)
                return BadRequest(new {message = "startDate is required." });
            if (request.EndDate.HasValue && request.EndDate.Value > DateTime.UtcNow)           
                return BadRequest(new { message = "endDate cannot be in the future." });      
            if (request.EndDate.HasValue && request.StartDate >= request.EndDate.Value)            
                return BadRequest(new { message = "startDate must be earlier than endDate." });
            
            try
            {
                var endDateStr = request.EndDate?.ToString("O") ?? "yesterday 23:59:59";
                _logger.LogInformation($"Download request in range : {request.Symbol} {request.Interval} from {request.StartDate:O} to {endDateStr}");

                var response = await _binanceKlineService.FetchAndSaveKlinesAsync(request, cancellationToken);

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation cancelled by user.");
                return StatusCode(499, new { message = "Request was cancelled by the client." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("get-db-klines")]
        public async Task<ActionResult<IReadOnlyList<DbCandleSummaryResponseDto>>> GetDbKlines()
        {
            return Ok(await _candleRepository.GetKlineDatasetAsync());
        }


        [HttpDelete("delete-db-klines")]
        public async Task<ActionResult<DbDatasetDeleteResponseDto>> DeleteKlines([FromBody] DbDatasetDeleteRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _binanceKlineService.DeleteKlinesAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("delete-batchresult")]
        public async Task<ActionResult<BacktestResultDeleteResponseDto>> DeleteBatchResult([FromBody] BacktestResultDeleteRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _backtestResultService.DeleteResultsFromDb(request, cancellationToken);
                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation cancelled by user.");
                return StatusCode(499, new { message = "Request was cancelled by the client." });
            }
            catch (KeyNotFoundException notfound)
            {
                _logger.LogError($"{notfound.Message}");
                return StatusCode(404, new {message = notfound.Message});
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("get-db-backtest-results")]
        public async Task<ActionResult<BacktestResultResponseDto[]>> DbBacktestResults()
        {
            return Ok(await _backtestResultService.BacktestResults());
        }


        [HttpGet("download-backtest-result")]
        public async Task<ActionResult> DownloadResultByGuid([FromQuery] Guid id)
        {
            var bytes = await _backtestResultService.DownloadBacktestResult(id);
            return File(
                bytes,
                "text/csv; charset=utf-8",
                $"backtest-results-{id}.csv");
        }

    }
}