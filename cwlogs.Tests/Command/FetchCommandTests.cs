using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using cwlogs.command;
using cwlogs.settings;
using FluentAssertions;
using Moq;
using Spectre.Console.Cli;
using Xunit;

namespace cwlogs.Tests.Command;

public class FetchCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithGroupName_CallsFilterLogEvents()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>();
        mockClient.Setup(c => c.FilterLogEventsAsync(It.IsAny<FilterLogEventsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FilterLogEventsResponse
            {
                Events = [new FilteredLogEvent { Message = "log1", Timestamp = 123456789 }]
            });

        var mockSettings = new Mock<FetchSettings>();
        mockSettings.SetupGet(s => s.GroupName).Returns("my-group");
        mockSettings.SetupGet(s => s.Sort).Returns("asc");
        mockSettings.SetupGet(s => s.Limit).Returns(10);
        mockSettings.Setup(s => s.CreateClient()).Returns(mockClient.Object);

        var command = new FetchCommand();
        var context = new CommandContext([], new Mock<IRemainingArguments>().Object, "fetch", null);

        var result = await command.ExecuteAsync(context, mockSettings.Object, CancellationToken.None);

        result.Should().Be(0);
        mockClient.Verify(c => c.FilterLogEventsAsync(It.Is<FilterLogEventsRequest>(r => 
            r.LogGroupName == "my-group" && r.Limit == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithStreams_CallsFilterLogEventsWithStreams()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>();
        mockClient.Setup(c => c.FilterLogEventsAsync(It.IsAny<FilterLogEventsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FilterLogEventsResponse { Events = [] });

        var mockSettings = new Mock<FetchSettings>();
        mockSettings.SetupGet(s => s.GroupName).Returns("my-group");
        mockSettings.SetupGet(s => s.Stream).Returns("stream1");
        mockSettings.SetupGet(s => s.Sort).Returns("asc");
        mockSettings.Setup(s => s.CreateClient()).Returns(mockClient.Object);

        var command = new FetchCommand();
        var context = new CommandContext([], new Mock<IRemainingArguments>().Object, "fetch", null);

        var result = await command.ExecuteAsync(context, mockSettings.Object, CancellationToken.None);

        result.Should().Be(0);
        mockClient.Verify(c => c.FilterLogEventsAsync(It.Is<FilterLogEventsRequest>(r => 
            r.LogGroupName == "my-group" && r.LogStreamNames.Contains("stream1")), It.IsAny<CancellationToken>()), Times.Once);
    }
}
