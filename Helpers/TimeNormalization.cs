

namespace BacktestingEngine.Helpers
{


    public static class TimeNormalization
    {

        // Normalizes a DateTime to the nearest lower multiple of the interval in milliseconds
        public static DateTime ToUtcOrAssumeUtc(this DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc), // "date wtihout zone = UTC"
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
        }


        public static long ToUnixMsUtc(this DateTime dt)
        {
            var utc = dt.ToUtcOrAssumeUtc();
            return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
        }

    }
}