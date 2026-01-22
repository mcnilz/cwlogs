using cwlogs.Base;
using cwlogs.settings;
using cwlogs.util;
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
            var groupName = settings.ResolvedGroupName;

            if (string.IsNullOrEmpty(groupName))
            {
                AnsiConsole.MarkupLine("[red]Error: Log group name must be specified.[/]");
                return 1;
            }

            var lastTimestamp = TimeUtils.ParseSince(settings.Since) ?? DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds();

            var streams = await ResolveStreams(client, groupName, settings.Stream, cancellationToken);

            AnsiConsole.MarkupLine($"[yellow]Tailing logs for {groupName}...[/]");

            while (!cancellationToken.IsCancellationRequested)
            {
                var request = new Amazon.CloudWatchLogs.Model.FilterLogEventsRequest
                {
                    LogGroupName = groupName,
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