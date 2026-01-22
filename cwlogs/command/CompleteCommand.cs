using System.ComponentModel;
using Amazon.CloudWatchLogs;
using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class CompleteCommand : AsyncCommand<CompleteCommand.Settings>
{
    public class Settings : GlobalSettings
    {
        [CommandOption("--groups <GROUPS>")]
        [Description("Name der Log-Gruppe für Stream-Vervollständigung")]
        public string? GroupName { get; init; }
        
        [CommandOption("--type <TYPE>")]
        [Description("Typ der Vervollständigung (groups, streams)")]
        public string? Type { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            using var client = settings.CreateClient();

            if (settings.Type == "groups")
            {
                var response = await client.DescribeLogGroupsAsync(new Amazon.CloudWatchLogs.Model.DescribeLogGroupsRequest());
                foreach (var group in response.LogGroups)
                {
                    Console.WriteLine(group.LogGroupName);
                }
            }
            else if (settings.Type == "streams" && !string.IsNullOrEmpty(settings.GroupName))
            {
                var response = await client.DescribeLogStreamsAsync(new Amazon.CloudWatchLogs.Model.DescribeLogStreamsRequest
                {
                    LogGroupName = settings.GroupName,
                    OrderBy = OrderBy.LastEventTime,
                    Descending = true,
                    Limit = 50
                });
                foreach (var stream in response.LogStreams)
                {
                    Console.WriteLine(stream.LogStreamName);
                }
            }

            return 0;
        }
        catch
        {
            return 1;
        }
    }
}
