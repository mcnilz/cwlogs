using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.settings;

public class CompletionSettings : GlobalSettings
{
    [CommandArgument(0, "[SHELL]")]
    [Description("Die Shell, für die das Completion-Script generiert werden soll (powershell, bash).")]
    public string? Shell { get; [UsedImplicitly] init; }
}
