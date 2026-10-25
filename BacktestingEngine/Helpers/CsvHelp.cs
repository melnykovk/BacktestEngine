using CsvHelper.Configuration;
using System.Globalization;
using BacktestingEngine.Engine.Core;
public sealed class BacktestStatsMap : ClassMap<BacktestStats>
{
    public BacktestStatsMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.StartDate).TypeConverterOption.Format("yyyy-MM-dd'T'HH:mm:ss");
        Map(x => x.EndDate).TypeConverterOption.Format("yyyy-MM-dd'T'HH:mm:ss");
    }
}