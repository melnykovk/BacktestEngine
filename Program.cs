using BacktestingEngine.Data;
using BacktestingEngine.BackgroundServices;
using BacktestingEngine.DTO;
using Microsoft.EntityFrameworkCore;
using BacktestingEngine.Engine;
using BacktestingEngine.Services;
using BacktestingEngine.Engine.Grid;
using BacktestingEngine.Engine.Core;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHttpClient<IBinanceKlineService, BinanceKlineService>();
builder.Services.AddScoped<ICandleRepository, CandleRepository>();
builder.Services.AddScoped<IKlinesUploadFromDbService, KlinesUploadFromDbService>();
builder.Services.AddScoped<EngineBuilder>();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddSingleton<TradeStore>();
builder.Services.AddSingleton<BinanceSocketService>(); // singleton для доступа к экземпляру
builder.Services.AddHostedService(provider => provider.GetRequiredService<BinanceSocketService>());

builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<TradesHub>("/tradesHub");
app.MapGet("/configmanager", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/configmanager.html");
});

app.MapGet("/dbmanager", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/dbmanager.html");
});
app.MapGet("/testing", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/layout.html");
});
app.UseHttpsRedirection();
app.MapControllers();
app.UseWebSockets();
app.UseDefaultFiles();  // автоматически отдаёт index.html при заходе на /
app.UseStaticFiles();   // отдаёт все файлы из wwwroot
app.Run();
