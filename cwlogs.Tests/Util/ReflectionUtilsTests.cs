using cwlogs.command;
using cwlogs.util;
using FluentAssertions;
using Xunit;

namespace cwlogs.Tests.Util;

public class ReflectionUtilsTests
{
    [Fact]
    public void GetCommandOptions_ForGroupsCommand_ReturnsExpectedOptions()
    {
        var options = ReflectionUtils.GetCommandOptions(typeof(GroupsCommand));
        
        // GroupsCommand uses GlobalSettings
        options.Should().Contain("-p");
        options.Should().Contain("--profile");
        options.Should().Contain("-r");
        options.Should().Contain("--region");
        options.Should().Contain("--no-color");
    }

    [Fact]
    public void GetCommandOptions_ForFetchCommand_ReturnsExpectedOptions()
    {
        var options = ReflectionUtils.GetCommandOptions(typeof(FetchCommand));
        
        // FetchCommand uses FetchSettings (inherits from StreamsSettings and GlobalSettings)
        options.Should().Contain("-p");
        options.Should().Contain("--profile");
        options.Should().Contain("-g");
        options.Should().Contain("--group");
        options.Should().Contain("-s");
        options.Should().Contain("--stream");
        options.Should().Contain("-l");
        options.Should().Contain("--limit");
        options.Should().Contain("--json");
        options.Should().Contain("--since");
    }

    [Fact]
    public void GetCommandOptions_ForCompletionCommand_ReturnsExpectedOptions()
    {
        var options = ReflectionUtils.GetCommandOptions(typeof(CompletionCommand));
        
        // CompletionCommand uses CompletionSettings (inherits from GlobalSettings)
        options.Should().Contain("-p");
        options.Should().Contain("--profile");
    }
}
