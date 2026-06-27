using GrindBotAPI.Data;
using GrindBotAPI.Entity;
using GrindBotAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using GrindBotAPI.DTO;
namespace GrindBotAPI.Services
{
    // Interface for KlineService to allow for easier testing and separation of concerns
    public interface IBinanceKlineService
    {
        Task<DbUploadKlinesResponseDto> FetchAndSaveKlinesFromDateAsync(DbUploadKlinesRequestDto request);
    }

    // Service responsible for fetching klines from Binance API and saving to database
    public class BinanceKlineService : IBinanceKlineService
    {
        // Dependencies are injected via constructor
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<BinanceKlineService> _logger;
        private const string BinanceApiUrl = "https://api.binance.com/api/v3";
        public BinanceKlineService(HttpClient httpClient, AppDbContext dbContext, ILogger<BinanceKlineService> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _logger = logger;
        }


        // Private function to fetch klines from Binance API
        private async Task<List<decimal[]>> FetchFromBinanceAsync(string symbol, string interval, int limit, long? startTime = null)
        {
            var url = $"{BinanceApiUrl}/klines?symbol={symbol}&interval={interval}&limit={limit}";
            _logger.LogDebug($"Binance API URL: {url}");

            if (startTime.HasValue)
            {
                url += $"&startTime={startTime}";
            }

            try
            {
                // Send GET request to Binance API
                var response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();
                // Read and parse the response content as a list of decimal arrays
                var klines = await response.Content.ReadFromJsonAsync<List<decimal[]>>();
                return klines ?? new List<decimal[]>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching klines from Binance: {ex.Message}");
                throw;
            }
        }


        // Private function to save klines to database
        private async Task<DbUploadKlinesResponseDto> SaveKlinesToDatabaseAsync(List<decimal[]> binanceKlines, DbUploadKlinesRequestDto request)
        {
            // Convert Binance klines to KlineEntity objects for database storage
            var newKlines = new List<KlineEntity>();

            foreach (var kline in binanceKlines)
            {
                var entity = new KlineEntity
                {
                    Symbol = request.Symbol,
                    Interval = request.Interval,
                    OpenTimeMs = (long)kline[0],
                    Open = kline[1],
                    High = kline[2],
                    Low = kline[3],
                    Close = kline[4],
                    Volume = kline[5],
                    CloseTimeMs = (long)kline[6],
                    QuoteAssetVol = kline[7],
                    TradesCount = (long)kline[8],
                    TakerBuyBase = kline[9],
                    TakerBuyQuote = kline[10]
                };

                newKlines.Add(entity);
            }

            // Extract keys from new klines to check for existing records in the database
            var newKeys = newKlines
            .Select(k => new { k.Symbol, k.Interval, k.OpenTimeMs })
            .ToList();

            // Query the database for existing klines with the same keys to avoid duplicates
            var openTimes = newKlines.Select(k => k.OpenTimeMs).ToList();

            var existingKlines = await _dbContext.BinanceKlines
            .Where(x => x.Symbol == request.Symbol
                && x.Interval == request.Interval
                && openTimes.Contains(x.OpenTimeMs))
            .ToListAsync();

            var existingDict = existingKlines.ToDictionary(
                k => new { k.Symbol, k.Interval, k.OpenTimeMs });

            int addCount = 0;
            int updateCount = 0;

            foreach (var newKline in newKlines)
            {
                var key = new { newKline.Symbol, newKline.Interval, newKline.OpenTimeMs };

                if (existingDict.ContainsKey(key))
                {
                    // Update a kline that already exists in the database with new data from Binance
                    var existing = existingDict[key];

                    existing.Open = newKline.Open;
                    existing.High = newKline.High;
                    existing.Low = newKline.Low;
                    existing.Close = newKline.Close;
                    existing.Volume = newKline.Volume;
                    existing.CloseTimeMs = newKline.CloseTimeMs;
                    existing.QuoteAssetVol = newKline.QuoteAssetVol;
                    existing.TradesCount = newKline.TradesCount;
                    existing.TakerBuyBase = newKline.TakerBuyBase;
                    existing.TakerBuyQuote = newKline.TakerBuyQuote;

                    _dbContext.BinanceKlines.Update(existing);
                    updateCount++;
                }
                else
                {
                    // Add a new kline to the database if it does not already exist
                    _dbContext.BinanceKlines.Add(newKline);
                    addCount++;
                }
            }

            await _dbContext.SaveChangesAsync();
            var response = new DbUploadKlinesResponseDto();
            response.Symbol = request.Symbol;
            response.Interval = request.Interval;
            response.StartDate = request.StartDate;
            response.EndDate = request.EndDate;
            response.FirstCandleTime = newKlines.Any() ? DateTimeOffset.FromUnixTimeMilliseconds((long)newKlines.First().OpenTimeMs).UtcDateTime : DateTime.MinValue;
            response.LastCandleTime = newKlines.Any() ? DateTimeOffset.FromUnixTimeMilliseconds((long)newKlines.Last().OpenTimeMs).UtcDateTime : DateTime.MinValue;
            response.AddedNewCandles = addCount;
            response.UpdatedExistingCandles = updateCount;
            _logger.LogInformation($"Database update complete: {addCount} added, {updateCount} updated for {request.Symbol} with interval {request.Interval}");

            return response;
        }


        // Placeholder for future implementation of fetching klines from a specific date range
        public async Task<DbUploadKlinesResponseDto> FetchAndSaveKlinesFromDateAsync(DbUploadKlinesRequestDto request)
        {
            try
            {
                var response = new DbUploadKlinesResponseDto();
                request.Symbol = request.Symbol.ToUpper();

                // if endDate is not provided, will use today date untill 00:00:00 (meant yesterday 23:59:59 w/o bugs)
                request.EndDate ??= DateTime.UtcNow.AddDays(-1).Date;

                var startTimeMs = TimeRange.StartOfDayMsUtc(request.StartDate);
                var endTimeMs = TimeRange.EndOfDayExclusiveMsUtc(request.EndDate.Value);



                var allKlines = new List<decimal[]>();
                long currentStartTime = startTimeMs;
                int batchCount = 0;

                while (currentStartTime <= endTimeMs)
                {
                    // Request 1000 candles by currentStartTime
                    var batch = await FetchFromBinanceAsync(
                        request.Symbol,
                        request.Interval,
                        1000,
                        currentStartTime);

                    if (!batch.Any())
                    {
                        _logger.LogInformation($"No more data returned from Binance API. Ending download.");
                        break;
                    }

                    batchCount++;
                    _logger.LogInformation($"Batch {batchCount}: Downloaded {batch.Count} candles");


                    var filteredBatch = batch
                        .Where(k => (long)k[0] >= startTimeMs && (long)k[0] <= endTimeMs)
                        .ToList();

                    allKlines.AddRange(filteredBatch);


                    var lastKlineTime = (long)batch.Last()[0];
                    if (lastKlineTime >= endTimeMs)
                    {
                        _logger.LogInformation($"Reached the end date");
                        break;
                    }

                    // Начинаем со следующей свечи
                    currentStartTime = (long)batch.Last()[0] + 1;

                    // Delay for API rate limiting (50ms)
                    await Task.Delay(50);
                }

                if (allKlines.Any())
                {
                    response = await SaveKlinesToDatabaseAsync(allKlines, request);
                    _logger.LogInformation($"Downloaded {request.Symbol} {request.Interval} from {request.StartDate:O} to {request.EndDate:O}");
                    _logger.LogInformation($"In ms: {startTimeMs} to {endTimeMs}");
                    _logger.LogInformation($"Added {allKlines.Count} klines to database for {request.Symbol} with interval {request.Interval}");
                }
                else
                {
                    _logger.LogWarning($"No klines found in the specified date range for symbol {request.Symbol} and interval {request.Interval}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                throw;
            }
        }


    }
}
