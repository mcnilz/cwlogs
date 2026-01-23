using cwlogs.command;
using cwlogs.util;
using FluentAssertions;

namespace cwlogs.Tests.Util;

public class ReflectionUtilsTests
{
    [Fact]
    public void GetCommandOptions_ForGroupsCommand_ReturnsExpectedOptions()
    {
        var options = ReflectionUtils.GetCommandOptionsDetailed(typeof(GroupsCommand)).Select(o => o.Name).ToList();
        
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
        var optionsDetailed = ReflectionUtils.GetCommandOptionsDetailed(typeof(FetchCommand));
        var options = optionsDetailed.Select(o => o.Name).ToList();
        
        // FetchCommand uses FetchSettings (inherits from StreamsSettings and GlobalSettings)
        options.Should().Contain("-p");
        options.Should().Contain("--profile");
        options.Should().Contain("-r");
        options.Should().Contain("--region");
        options.Should().Contain("--no-color");
        options.Should().Contain("-g");
        options.Should().Contain("--group");
        options.Should().Contain("-s");
        options.Should().Contain("--stream");
        options.Should().Contain("-l");
        options.Should().Contain("--limit");
        options.Should().Contain("--sort");
        options.Should().Contain("--single-line");
        options.Should().Contain("--raw");
        options.Should().Contain("--clean");
        options.Should().Contain("--json");
        options.Should().Contain("--since");

        optionsDetailed.First(o => o.Name == "--single-line").IsFlag.Should().BeTrue();
        optionsDetailed.First(o => o.Name == "--limit").IsFlag.Should().BeFalse();
    }

    [Fact]
    public void GetCommandOptions_ForCompletionCommand_ReturnsExpectedOptions()
    {
        var options = ReflectionUtils.GetCommandOptionsDetailed(typeof(CompletionCommand)).Select(o => o.Name).ToList();
        
        // CompletionCommand uses CompletionSettings (inherits from GlobalSettings)
        options.Should().Contain("-p");
        options.Should().Contain("--profile");
    }
}
