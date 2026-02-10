using cwlogs.command;
using cwlogs.util;

namespace cwlogs.Tests.Util;

public class ReflectionUtilsTests
{
    [Fact]
    public void GetCommandOptions_ForGroupsCommand_ReturnsExpectedOptions()
    {
        var options = ReflectionUtils.GetCommandOptionsDetailed(typeof(GroupsCommand)).Select(o => o.Name).ToList();

        // GroupsCommand uses GlobalSettings
        Assert.Contains("-p", options);
        Assert.Contains("--profile", options);
        Assert.Contains("-r", options);
        Assert.Contains("--region", options);
        Assert.Contains("--no-color", options);
    }

    [Fact]
    public void GetCommandOptions_ForFetchCommand_ReturnsExpectedOptions()
    {
        var optionsDetailed = ReflectionUtils.GetCommandOptionsDetailed(typeof(FetchCommand));
        var options = optionsDetailed.Select(o => o.Name).ToList();

        // FetchCommand uses FetchSettings (inherits from StreamsSettings and GlobalSettings)
        Assert.Contains("-p", options);
        Assert.Contains("--profile", options);
        Assert.Contains("-r", options);
        Assert.Contains("--region", options);
        Assert.Contains("--no-color", options);
        Assert.Contains("-g", options);
        Assert.Contains("--group", options);
        Assert.Contains("-s", options);
        Assert.Contains("--stream", options);
        Assert.Contains("-l", options);
        Assert.Contains("--limit", options);
        Assert.Contains("--sort", options);
        Assert.Contains("--single-line", options);
        Assert.Contains("--raw", options);
        Assert.Contains("--clean", options);
        Assert.Contains("--json", options);
        Assert.Contains("--since", options);

        Assert.True(optionsDetailed.First(o => o.Name == "--single-line").IsFlag);
        Assert.False(optionsDetailed.First(o => o.Name == "--limit").IsFlag);
    }

    [Fact]
    public void GetCommandOptions_ForCompletionCommand_ReturnsExpectedOptions()
    {
        var options = ReflectionUtils.GetCommandOptionsDetailed(typeof(CompletionCommand)).Select(o => o.Name).ToList();

        // CompletionCommand uses CompletionSettings (inherits from GlobalSettings)
        Assert.Contains("-p", options);
        Assert.Contains("--profile", options);
    }
}
