using System.Diagnostics.CodeAnalysis;
using Amazon.Runtime;
using Spectre.Console;

namespace cwlogs.util;

public static class ErrorHandler
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Spectre.Console's WriteException is used for error reporting. While not fully AOT-safe, it provides valuable debugging info.")]
    public static int HandleException(Exception ex)
    {
        if (ex is AmazonClientException)
        {
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
            return 1;
        }

        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        return 1;
    }
}