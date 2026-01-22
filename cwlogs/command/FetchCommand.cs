using Amazon.CloudWatchLogs;
using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class FetchCommand : AsyncCommand<FetchSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, FetchSettings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = settings.CreateClient();
            var streams = await ResolveStreams(client, settings.GroupName, settings.Stream, cancellationToken);

            var descending = settings.Sort.Equals("desc", StringComparison.OrdinalIgnoreCase);

            if (streams == null || streams.Count == 0)
            {
                var response = await client.FilterLogEventsAsync(new Amazon.CloudWatchLogs.Model.FilterLogEventsRequest
                {
                    LogGroupName = settings.GroupName,
                    Limit = settings.Limit
                }, cancellationToken);

                var events = response.Events;
                events = descending
                    ? events.OrderByDescending(e => e.Timestamp).ToList()
                    : events.OrderBy(e => e.Timestamp).ToList();

                foreach (var ev in events)
                {
                    PrintLogEvent(ev, settings);
                }
            }
            else
            {
                // If multiple streams are resolved (e.g. from index), we use FilterLogEvents
                var request = new Amazon.CloudWatchLogs.Model.FilterLogEventsRequest
                {
                    LogGroupName = settings.GroupName,
                    LogStreamNames = streams,
                    Limit = settings.Limit
                };

                var response = await client.FilterLogEventsAsync(request, cancellationToken);
                var events = response.Events;
                events = descending
                    ? events.OrderByDescending(e => e.Timestamp).ToList()
                    : events.OrderBy(e => e.Timestamp).ToList();

                foreach (var ev in events)
                {
                    PrintLogEvent(ev, settings);
                }
            }

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

        if (int.TryParse(streamParam, out var n))
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
        // Das sind 3 Segmente getrennt durch Tabs oder Leerzeichen
        var parts = message.Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
        {
            // Versuche zu erkennen ob das erste ein Timestamp ist (sehr grob: enthält 'T' und 'Z')
            // Und das zweite eine GUID (RequestID)
            if (parts[0].Contains('T') && parts[0].EndsWith('Z') && parts[1].Contains('-'))
            {
                var foundCount = 0;
                var currentPos = 0;

                // Wir suchen das Ende des 3. Segments
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