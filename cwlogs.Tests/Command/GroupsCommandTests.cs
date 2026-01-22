using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using cwlogs.command;
using cwlogs.settings;
using FluentAssertions;
using Moq;
using Spectre.Console.Cli;

namespace cwlogs.Tests.Command;

public class GroupsCommandTests
{
    [Fact]
    public async Task ExecuteAsync_CallsDescribeLogGroups()
    {
        var mockClient = new Mock<IAmazonCloudWatchLogs>(); 
        mockClient.Setup(c => c.DescribeLogGroupsAsync(It.IsAny<DescribeLogGroupsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeLogGroupsResponse
            {
                LogGroups = [new LogGroup { LogGroupName = "group1" }]
            });

        var settings = new Mock<GlobalSettings>();
        settings.Setup(s => s.CreateClient()).Returns(mockClient.Object);

        var command = new GroupsCommand();
        var context = new CommandContext([], new Mock<IRemainingArguments>().Object, "groups", null);

        var result = await command.ExecuteAsync(context, settings.Object, CancellationToken.None);

        result.Should().Be(0);
        mockClient.Verify(c => c.DescribeLogGroupsAsync(It.IsAny<DescribeLogGroupsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
