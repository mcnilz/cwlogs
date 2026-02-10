using cwlogs.util;

namespace cwlogs.Tests.Util;

public class LambdaUtilsTests
{
    [Fact]
    public void CleanLambdaMessage_ValidLambdaHeader_ReturnsCleanedMessage()
    {
        var message = "2026-01-22T08:24:52.392Z 0fbd43a3-1435-4cc0-8209-2de3b5f1a461 INFO Hello World";
        var result = LambdaUtils.CleanLambdaMessage(message);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void CleanLambdaMessage_TabSeparator_ReturnsCleanedMessage()
    {
        var message = "2026-01-22T08:24:52.392Z\t0fbd43a3-1435-4cc0-8209-2de3b5f1a461\tINFO\tHello World";
        var result = LambdaUtils.CleanLambdaMessage(message);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void CleanLambdaMessage_NoLambdaHeader_ReturnsOriginalMessage()
    {
        var message = "Just a normal log message";
        var result = LambdaUtils.CleanLambdaMessage(message);
        Assert.Equal(message, result);
    }

    [Fact]
    public void CleanLambdaMessage_PartialHeader_ReturnsOriginalMessage()
    {
        var message = "2026-01-22T08:24:52.392Z INFO Hello World";
        var result = LambdaUtils.CleanLambdaMessage(message);
        Assert.Equal(message, result);
    }
}
