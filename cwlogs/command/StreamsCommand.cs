using Amazon.CloudWatchLogs;
using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class StreamsCommand : AsyncCommand<StreamsSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, StreamsSettings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = settings.CreateClient();
            var response = await client.DescribeLogStreamsAsync(
                new Amazon.CloudWatchLogs.Model.DescribeLogStreamsRequest
                {
                    LogGroupName = settings.GroupName,
                    OrderBy = OrderBy.LastEventTime,
                    Descending = true
                }, cancellationToken);

            var table = new Table().NoBorder();
            table.AddColumn("Log Stream Name");

            foreach (var stream in response.LogStreams)
            {
                table.AddRow(Markup.Escape(stream.LogStreamName ?? string.Empty));
            }

            AnsiConsole.Write(table);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}