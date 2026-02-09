using cwlogs.settings;
using cwlogs.shell;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class CompletionCommand : AsyncCommand<CompletionSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, CompletionSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(settings.Shell))
        {
            AnsiConsole.MarkupLine("[red]Error: Please specify a shell (powershell, bash).[/]");
            AnsiConsole.WriteLine("Example: cwlogs completion powershell");
            return Task.FromResult(1);
        }

        var exePath = Environment.ProcessPath ?? "cwlogs";
        var exeName = System.IO.Path.GetFileNameWithoutExtension(exePath);

        if (settings.Shell.Equals("bash", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.WriteLine(BashCompletionGenerator.Generate(exePath, exeName));
        }
        else if (settings.Shell.Equals("powershell", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.WriteLine(PowerShellCompletionGenerator.Generate(exePath, exeName));
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Error: Unsupported shell '{settings.Shell}'. Supported are: powershell, bash.[/]");
            return Task.FromResult(1);
        }

        return Task.FromResult(0);
    }
}
