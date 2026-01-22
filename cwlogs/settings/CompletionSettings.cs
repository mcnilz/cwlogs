using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.settings;

public class CompletionSettings : GlobalSettings
{
    [CommandArgument(0, "[SHELL]")]
    [Description("The shell for which the completion script should be generated (powershell, bash).")]
    public string? Shell { get; [UsedImplicitly] init; }
}
