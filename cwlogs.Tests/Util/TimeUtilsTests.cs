using cwlogs.util;
using FluentAssertions;

namespace cwlogs.Tests.Util;

public class TimeUtilsTests
{
    [Fact]
    public void ParseSince_Null_ReturnsNull()
    {
        TimeUtils.ParseSince(null).Should().BeNull();
    }

    [Fact]
    public void ParseSince_Minutes_ReturnsCorrectTimestamp()
    {
        var result = TimeUtils.ParseSince("10m");
        result.Should().NotBeNull();
        var expected = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeMilliseconds();
        result.Value.Should().BeCloseTo(expected, 1000);
    }

    [Fact]
    public void ParseSince_Hours_ReturnsCorrectTimestamp()
    {
        var result = TimeUtils.ParseSince("1h");
        result.Should().NotBeNull();
        var expected = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds();
        result.Value.Should().BeCloseTo(expected, 1000);
    }

    [Fact]
    public void ParseSince_Days_ReturnsCorrectTimestamp()
    {
        var result = TimeUtils.ParseSince("1d");
        result.Should().NotBeNull();
        var expected = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();
        result.Value.Should().BeCloseTo(expected, 1000);
    }

    [Fact]
    public void ParseSince_AbsoluteDate_ReturnsCorrectTimestamp()
    {
        var result = TimeUtils.ParseSince("2026-01-22");
        result.Should().NotBeNull();
        var expected = DateTimeOffset.Parse("2026-01-22").ToUnixTimeMilliseconds();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void ParseSince_Invalid_ReturnsNull()
    {
        TimeUtils.ParseSince("invalid").Should().BeNull();
    }
}
