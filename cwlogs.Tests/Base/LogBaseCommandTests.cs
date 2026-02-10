using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using cwlogs.Base;
using Moq;

namespace cwlogs.Tests.Base;

public class LogBaseCommandTests
{
    [Fact]
    public async Task ResolveStreams_NullParam_ReturnsNull()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>();
        var result = await LogBaseCommand<cwlogs.settings.FetchSettings>.ResolveStreams(mockClient.Object, "group", null, CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveStreams_SpecificStream_ReturnsStream()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>();
        var result = await LogBaseCommand<cwlogs.settings.FetchSettings>.ResolveStreams(mockClient.Object, "group", "my-stream", CancellationToken.None);
        Assert.Single(result!);
        Assert.Equal("my-stream", result![0]);
    }

    [Fact]
    public async Task ResolveStreams_NumberParam_CallsDescribeLogStreams()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>();
        mockClient.Setup(c => c.DescribeLogStreamsAsync(It.IsAny<DescribeLogStreamsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeLogStreamsResponse
            {
                LogStreams = [new LogStream { LogStreamName = "stream1" }, new LogStream { LogStreamName = "stream2" }]
            });

        var result = await LogBaseCommand<cwlogs.settings.FetchSettings>.ResolveStreams(mockClient.Object, "group", "2", CancellationToken.None);

        Assert.Equal(2, result!.Count);
        Assert.Equal("stream1", result![0]);
        Assert.Equal("stream2", result![1]);

        mockClient.Verify(c => c.DescribeLogStreamsAsync(It.Is<DescribeLogStreamsRequest>(r =>
            r.LogGroupName == "group" && r.Limit == 2 && r.OrderBy == OrderBy.LastEventTime && r.Descending == true), It.IsAny<CancellationToken>()), Times.Once);
    }
}
