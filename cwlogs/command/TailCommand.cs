using Amazon.CloudWatchLogs;
using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class TailCommand : AsyncCommand<FetchSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, FetchSettings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = settings.CreateClient();
            var lastTimestamp = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds();

            var streams = await ResolveStreams(client, settings.GroupName, settings.Stream, cancellationToken);

            AnsiConsole.MarkupLine($"[yellow]Tailing logs for {settings.GroupName}...[/]");

            while (!cancellationToken.IsCancellationRequested)
            {
                var request = new Amazon.CloudWatchLogs.Model.FilterLogEventsRequest
                {
                    LogGroupName = settings.GroupName,
                    StartTime = lastTimestamp + 1,
                    Limit = settings.Limit
                };

                if (streams is { Count: > 0 })
                {
                    request.LogStreamNames = streams;
                }

                var response = await client.FilterLogEventsAsync(request, cancellationToken);

                foreach (var ev in response.Events.OrderBy(e => e.Timestamp))
                {
                    PrintLogEvent(ev, settings);
                    if (ev.Timestamp > lastTimestamp)
                    {
                        lastTimestamp = ev.Timestamp ?? lastTimestamp;
                    }
                }

                await Task.Delay(2000, cancellationToken);
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static async Task<List<string>?> ResolveStreams(AmazonCloudWatchLogsClient client, string groupName,
        string? streamParam, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(streamParam)) return null;

        if (int.TryParse(streamParam, out int n))
        {
            var response = await client.DescribeLogStreamsAsync(
                new Amazon.CloudWatchLogs.Model.DescribeLogStreamsRequest
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

    private static void PrintLogEvent(Amazon.CloudWatchLogs.Model.FilteredLogEvent ev, FetchSettings settings)
    {
        var message = ev.Message.TrimEnd();

        if (settings.Clean)
        {
            message = CleanLambdaMessage(message);
        }

        if (settings.SingleLine) message = message.Replace("\r", "").Replace("\n", " ");

        if (settings.Raw)
        {
            Console.WriteLine(message);
        }
        else
        {
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(ev.Timestamp ?? 0).DateTime.ToLocalTime();
            AnsiConsole.MarkupLine($"[{Color.Grey}]{dt:yyyy-MM-dd HH:mm:ss}[/] {Markup.Escape(message)}");
        }
    }

    private static string CleanLambdaMessage(string message)
    {
        // Beispiel: 2026-01-22T08:24:52.392Z 0fbd43a3-1435-4cc0-8209-2de3b5f1a461 INFO
        var parts = message.Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
        {
            if (parts[0].Contains('T') && parts[0].EndsWith('Z') && parts[1].Contains('-'))
            {
                var foundCount = 0;
                var currentPos = 0;

                while (foundCount < 3 && currentPos < message.Length)
                {
                    while (currentPos < message.Length && !char.IsWhiteSpace(message[currentPos])) currentPos++;
                    foundCount++;
                    while (currentPos < message.Length && char.IsWhiteSpace(message[currentPos])) currentPos++;
                }

                if (foundCount >= 3)
                {
                    return message.Substring(currentPos).TrimStart();
                }
            }
        }

        return message;
    }
}