using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.settings;

public class StreamsSettings : GlobalSettings
{
    [CommandArgument(0, "<GROUP_NAME>")] public string GroupName { get; [UsedImplicitly] init; } = string.Empty;
}