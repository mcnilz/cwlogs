using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.settings;

public class StreamsSettings : GlobalSettings
{
    [CommandArgument(0, "[GROUP_NAME]")] 
    [Description("Name of the log group (optional if --group is used).")]
    public string? GroupName { get; [UsedImplicitly] init; }

    [CommandOption("-g|--group <GROUP>")]
    [Description("Name of the log group.")]
    public string? GroupOption { get; [UsedImplicitly] init; }

    public string? ResolvedGroupName => !string.IsNullOrEmpty(GroupOption) ? GroupOption : GroupName;
}