namespace GrindBotAPI.Helpers
{

    public static class TimeRange
    {
        public static DateTime EndOfDayExclusiveUtc(DateTime date)
            => date.ToUtcOrAssumeUtc().Date.AddDays(1);

        public static DateTime StartOfDayUtc(DateTime date)
        => date.ToUtcOrAssumeUtc().Date;

        public static long StartOfDayMsUtc(DateTime date)
        => StartOfDayUtc(date).ToUnixMsUtc();

        public static long EndOfDayExclusiveMsUtc(DateTime date)
        => EndOfDayExclusiveUtc(date).ToUnixMsUtc();
    }
}
