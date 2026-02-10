using cwlogs.util;

namespace cwlogs.Tests.Util;

public class TimeUtilsTests
{
    [Fact]
    public void ParseSince_Null_ReturnsNull()
    {
        Assert.Null(TimeUtils.ParseSince(null));
    }

    [Fact]
    public void ParseSince_Minutes_ReturnsCorrectTimestamp()
    {
        var result = TimeUtils.ParseSince("10m");
        Assert.NotNull(result);
        var expected = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeMilliseconds();
        Assert.True(Math.Abs(expected - result.Value) <= 1000);
    }

    [Fact]
    public void ParseSince_Hours_ReturnsCorrectTimestamp()
    {
        var result = TimeUtils.ParseSince("1h");
        Assert.NotNull(result);
        var expected = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds();
        Assert.True(Math.Abs(expected - result.Value) <= 1000);
    }

    [Fact]
    public void ParseSince_Days_ReturnsCorrectTimestamp()
    {
        var result = TimeUtils.ParseSince("1d");
        Assert.NotNull(result);
        var expected = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();
        Assert.True(Math.Abs(expected - result.Value) <= 1000);
    }

    [Fact]
    public void ParseSince_AbsoluteDate_ReturnsCorrectTimestamp()
    {
        var result = TimeUtils.ParseSince("2026-01-22");
        Assert.NotNull(result);
        var expected = DateTimeOffset.Parse("2026-01-22").ToUnixTimeMilliseconds();
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void ParseSince_Invalid_ReturnsNull()
    {
        Assert.Null(TimeUtils.ParseSince("invalid"));
    }
}
