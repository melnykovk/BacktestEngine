using GrindBotAPI.BackgroundServices;
using GrindBotAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using GrindBotAPI.Services;

namespace GrindBotAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly TradeStore _store;
        private readonly IBinanceKlineService _binanceKlineService;
        private readonly ILogger<BinanceController> _logger;
        private readonly IKlinesUploadFromDbService _klinesUploadFromDbService;

        public UserController(TradeStore store, IBinanceKlineService binanceKlineService, ILogger<BinanceController> logger, IKlinesUploadFromDbService klinesUploadFromDbService)
        {
            _store = store;
            _binanceKlineService = binanceKlineService;
            _logger = logger;
            _klinesUploadFromDbService = klinesUploadFromDbService;
        }

        [HttpGet("get-db-klines")]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult Post()
        {
            return Ok("POST request received!");
        }
    }
}