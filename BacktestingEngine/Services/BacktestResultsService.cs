using BacktestingEngine.Data;
using BacktestingEngine.Engine;
using BacktestingEngine.DTO;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public interface IBacktestReusltService
{
    Task<BacktestResultResponseDto> RunBacktestAndSaveResult(BacktestBatchRequest batchRequest, CancellationToken _token);
    Task<BacktestResultDeleteResponseDto> DeleteResultsFromDb(BacktestResultDeleteRequestDto request, CancellationToken _token);
    Task<List<BacktestResultResponseDto>> BacktestResults();
    Task<byte[]> DownloadBacktestResult(Guid request);
}

// Provides service for uploading and downloading backtests results to and form the database and updating dashboard data.
public class BacktestResultService : IBacktestReusltService
{
    private readonly AppDbContext _dbContext;
    private readonly EngineBuilder _engine;

    public BacktestResultService(AppDbContext dbContext, EngineBuilder engine)
    {
        _dbContext = dbContext;
        _engine = engine;
    }

    public async Task<BacktestResultResponseDto> RunBacktestAndSaveResult(BacktestBatchRequest batchRequest, CancellationToken _token)
    {
        var result = await _engine.BacktestRunnerBatchResult(batchRequest, _token);
        await SaveResultsToDb(result, _token);

        return new BacktestResultResponseDto
        {
            Id = result.Id,
            Name = result.Name!,
            CreatedAtUtc = result.CreatedAtUtc,
            Symbol = batchRequest.RangeData.Symbol,
            Interval = batchRequest.RangeData.Interval,
            StartDate = batchRequest.RangeData.StartDate,
            EndDate = batchRequest.RangeData.EndDate,
            RunsCount = result.Stats.Count,
            ExecutionTime = result.ExecutionTime
        };   
    }
    public async Task SaveResultsToDb(BacktestBatchResultEntity results, CancellationToken _token)
    {
        await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(_token);
        try
        {
            await _dbContext.BatchResults.AddAsync(results);
            await _dbContext.SaveChangesAsync(_token);
            await transaction.CommitAsync(_token);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<BacktestResultDeleteResponseDto> DeleteResultsFromDb(BacktestResultDeleteRequestDto request, CancellationToken _token)
    {

        var data = await _dbContext.BatchResults.Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync();
        if (data == null)
            throw new KeyNotFoundException($"Batch result with ID {request.Id} was not found.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(_token);
            try
            {
                await _dbContext.BatchResults.Where(x => x.Id == request.Id)
                .ExecuteDeleteAsync(_token);
                await _dbContext.SaveChangesAsync(_token);
                await transaction.CommitAsync(_token);
                
                return new BacktestResultDeleteResponseDto
                {
                    Id = data.Id,
                    Name = data.Name!,
                    Symbol = data.Symbol,
                    Interval = data.Interval,
                    StartDate = data.StartDate,
                    EndDate = data.EndDate,
                    RunsCount = data.RunsCount
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
    }

    public async Task<List<BacktestResultResponseDto>> BacktestResults()
    {
        var data = await _dbContext.BatchResults
        .AsNoTracking()
        .Select(k => new BacktestResultResponseDto
        {
            Id = k.Id,
            Name = k.Name!,
            Symbol = k.Symbol,
            Interval = k.Interval,
            StartDate = k.StartDate,
            EndDate = k.EndDate,
            RunsCount = k.RunsCount,
            ExecutionTime = k.ExecutionTime,
            CreatedAtUtc = k.CreatedAtUtc
        })
        .OrderByDescending(x => x.CreatedAtUtc)
        .ToListAsync();
        return data;
    }

    public async Task<byte[]> DownloadBacktestResult(Guid request)
    {
        var data = await _dbContext.BatchResults
        .AsNoTracking()
        .Where(x => x.Id == request)
        .Select(i => i.Stats)
        .FirstOrDefaultAsync();
        if(data is null)
            throw new KeyNotFoundException($"Batch result with ID {request} was not found.");
        
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        using(var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<BacktestStatsMap>();
            csv.WriteRecords(data);
        }
        var bytes = new UTF8Encoding(true).GetBytes(writer.ToString());

        return bytes;
    }

}