using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using cwlogs.command;
using cwlogs.settings;
using FluentAssertions;
using Moq;
using Spectre.Console.Cli;
using Xunit;

namespace cwlogs.Tests.Command;

public class StreamsCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithGroupName_CallsDescribeLogStreams()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>();
        mockClient.Setup(c => c.DescribeLogStreamsAsync(It.IsAny<DescribeLogStreamsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeLogStreamsResponse
            {
                LogStreams = [new LogStream { LogStreamName = "stream1" }]
            });

        var mockSettings = new Mock<StreamsSettings>();
        mockSettings.SetupGet(s => s.GroupName).Returns("my-group");
        mockSettings.Setup(s => s.CreateClient()).Returns(mockClient.Object);

        var command = new StreamsCommand();
        var context = new CommandContext([], new Mock<IRemainingArguments>().Object, "streams", null);

        var result = await command.ExecuteAsync(context, mockSettings.Object, CancellationToken.None);

        result.Should().Be(0);
        mockClient.Verify(c => c.DescribeLogStreamsAsync(It.Is<DescribeLogStreamsRequest>(r => r.LogGroupName == "my-group"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_MissingGroupName_ReturnsError()
    {
        var mockSettings = new Mock<StreamsSettings>();
        mockSettings.SetupGet(s => s.GroupName).Returns((string?)null);

        var command = new StreamsCommand();
        var context = new CommandContext([], new Mock<IRemainingArguments>().Object, "streams", null);

        var result = await command.ExecuteAsync(context, mockSettings.Object, CancellationToken.None);

        result.Should().Be(1);
    }
}
