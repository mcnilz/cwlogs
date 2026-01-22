using System;

namespace cwlogs.util;

public static class TimeUtils
{
    public static long? ParseSince(string? since)
    {
        if (string.IsNullOrEmpty(since)) return null;

        if (since.EndsWith("m"))
        {
            if (int.TryParse(since.Substring(0, since.Length - 1), out var m))
                return DateTimeOffset.UtcNow.AddMinutes(-m).ToUnixTimeMilliseconds();
        }

        if (since.EndsWith("h"))
        {
            if (int.TryParse(since.Substring(0, since.Length - 1), out var h))
                return DateTimeOffset.UtcNow.AddHours(-h).ToUnixTimeMilliseconds();
        }

        if (since.EndsWith("d"))
        {
            if (int.TryParse(since.Substring(0, since.Length - 1), out var d))
                return DateTimeOffset.UtcNow.AddDays(-d).ToUnixTimeMilliseconds();
        }

        if (DateTimeOffset.TryParse(since, out var dt))
        {
            return dt.ToUnixTimeMilliseconds();
        }

        return null;
    }
}
