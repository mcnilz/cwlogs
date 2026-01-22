using System.Text.Json;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using cwlogs.settings;
using cwlogs.util;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.Base;

public abstract class LogBaseCommand<TSettings> : AsyncCommand<TSettings> where TSettings : FetchSettings
{
    public static async Task<List<string>?> ResolveStreams(IAmazonCloudWatchLogs client, string groupName,
        string? streamParam, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(streamParam)) return null;

        if (int.TryParse(streamParam, out var n))
        {
            var response = await client.DescribeLogStreamsAsync(
                new DescribeLogStreamsRequest
                {
                    LogGroupName = groupName,
                    OrderBy = OrderBy.LastEventTime,
                    Descending = true,
                    Limit = n
                }, cancellationToken);

            return response.LogStreams.Select(s => s.LogStreamName).ToList();
        }

        return [streamParam];
    }

    protected static void PrintLogEvent(FilteredLogEvent ev, FetchSettings settings)
    {
        if (settings.Json)
        {
            var messageContent = JsonUtils.ParseMessage(ev.Message);

            var json = JsonSerializer.Serialize(new LogEventOutput
            {
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ev.Timestamp ?? 0).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Stream = ev.LogStreamName,
                Message = messageContent,
                EventId = ev.EventId
            }, SourceGenerationContext.Default.LogEventOutput);
            Console.WriteLine(json);
            return;
        }

        var message = ev.Message.TrimEnd();

        if (settings.Clean)
        {
            message = LambdaUtils.CleanLambdaMessage(message);
        }

        if (settings.SingleLine) message = message.Replace("\r", "").Replace("\n", " ");

        if (settings.Raw)
        {
            Console.WriteLine(message);
        }
        else
        {
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(ev.Timestamp ?? 0).DateTime.ToLocalTime();
            if (settings.NoColor)
            {
                Console.WriteLine($"{dt:yyyy-MM-dd HH:mm:ss} {message}");
            }
            else
            {
                AnsiConsole.MarkupLine($"[{Color.Grey}]{dt:yyyy-MM-dd HH:mm:ss}[/] {Markup.Escape(message)}");
            }
        }
    }
}
