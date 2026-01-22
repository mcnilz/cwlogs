using Amazon.CloudWatchLogs;
using cwlogs.Base;
using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class TailCommand : LogBaseCommand<FetchSettings>
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
}