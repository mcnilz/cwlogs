using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.settings;

public class StreamsSettings : GlobalSettings
{
    [CommandArgument(0, "[GROUP_NAME]")] 
    [Description("Name der Log-Gruppe (optional, wenn --group verwendet wird).")]
    public string? GroupName { get; [UsedImplicitly] init; }

    [CommandOption("-g|--group <GROUP>")]
    [Description("Name der Log-Gruppe.")]
    public string? GroupOption { get; [UsedImplicitly] init; }

    public string? ResolvedGroupName => !string.IsNullOrEmpty(GroupOption) ? GroupOption : GroupName;
}