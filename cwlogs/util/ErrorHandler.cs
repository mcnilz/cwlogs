using Amazon.Runtime;
using Spectre.Console;

namespace cwlogs.util;

public static class ErrorHandler
{
    public static int HandleException(Exception ex)
    {
        if (ex is AmazonClientException)
        {
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
            return 1;
        }

        AnsiConsole.WriteException(ex);
        return 1;
    }
}