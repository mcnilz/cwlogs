using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using cwlogs.Base;
using cwlogs.command;
using Moq;
using Spectre.Console.Cli;

namespace cwlogs.Tests.Command;

public class CompleteCommandTests
{
    [Fact]
    public async Task ExecuteAsync_MetadataType_ReturnsSuccess()
    {
        var command = new CompleteCommand();
        var settings = new CompleteCommand.Settings { Type = "metadata" };
        var context = new CommandContext([], Mock.Of<IRemainingArguments>(), "complete", null);

        var result = await command.ExecuteAsync(context, settings, CancellationToken.None);

        Assert.Equal(0, result);
        // Metadata doesn't use AWS client, so no need to mock it
    }

    [Fact]
    public async Task ExecuteAsync_GroupsType_CallsDescribeLogGroups()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>();
        mockClient.Setup(c => c.DescribeLogGroupsAsync(It.IsAny<DescribeLogGroupsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeLogGroupsResponse
            {
                LogGroups = [new LogGroup { LogGroupName = "group1" }]
            });

        var settingsMock = new Mock<CompleteCommand.Settings>();
        settingsMock.Setup(s => s.CreateClient()).Returns(mockClient.Object);
        settingsMock.SetupGet(s => s.Type).Returns(CommandNames.Groups);

        var command = new CompleteCommand();
        var context = new CommandContext([], Mock.Of<IRemainingArguments>(), "complete", null);

        var result = await command.ExecuteAsync(context, settingsMock.Object, CancellationToken.None);

        Assert.Equal(0, result);
        mockClient.Verify(c => c.DescribeLogGroupsAsync(It.IsAny<DescribeLogGroupsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_StreamsType_CallsDescribeLogStreams()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>();
        mockClient.Setup(c => c.DescribeLogStreamsAsync(It.IsAny<DescribeLogStreamsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeLogStreamsResponse
            {
                LogStreams = [new LogStream { LogStreamName = "stream1" }]
            });

        var settingsMock = new Mock<CompleteCommand.Settings>();
        settingsMock.Setup(s => s.CreateClient()).Returns(mockClient.Object);
        settingsMock.SetupGet(s => s.Type).Returns(CommandNames.Streams);
        settingsMock.SetupGet(s => s.GroupName).Returns("my-group");

        var command = new CompleteCommand();
        var context = new CommandContext([], Mock.Of<IRemainingArguments>(), "complete", null);

        var result = await command.ExecuteAsync(context, settingsMock.Object, CancellationToken.None);

        Assert.Equal(0, result);
        mockClient.Verify(c => c.DescribeLogStreamsAsync(It.Is<DescribeLogStreamsRequest>(r => r.LogGroupName == "my-group"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
