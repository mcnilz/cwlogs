using cwlogs.Base;
using cwlogs.settings;
using cwlogs.util;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class FetchCommand : LogBaseCommand<FetchSettings>
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

            var streams = await ResolveStreams(client, groupName, settings.Stream, cancellationToken);
            var startTime = TimeUtils.ParseSince(settings.Since);

            var descending = settings.Sort.Equals("desc", StringComparison.OrdinalIgnoreCase);

            if (streams == null || streams.Count == 0)
            {
                var request = new Amazon.CloudWatchLogs.Model.FilterLogEventsRequest
                {
                    LogGroupName = groupName,
                    Limit = settings.Limit
                };

                if (startTime.HasValue) request.StartTime = startTime.Value;

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
            else
            {
                // If multiple streams are resolved (e.g. from index), we use FilterLogEvents
                var request = new Amazon.CloudWatchLogs.Model.FilterLogEventsRequest
                {
                    LogGroupName = groupName,
                    LogStreamNames = streams,
                    Limit = settings.Limit
                };

                if (startTime.HasValue) request.StartTime = startTime.Value;

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
}