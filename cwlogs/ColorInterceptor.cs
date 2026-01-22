using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs;

[UsedImplicitly]
public class ColorInterceptor : ICommandInterceptor
{
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (settings is GlobalSettings { NoColor: true })
        {
            AnsiConsole.Profile.Capabilities.ColorSystem = ColorSystem.NoColors;
        }
    }
}