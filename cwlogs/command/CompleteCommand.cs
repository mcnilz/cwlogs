using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Amazon.CloudWatchLogs;
using cwlogs.Base;
using cwlogs.settings;
using cwlogs.util;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class CompleteCommand : AsyncCommand<CompleteCommand.Settings>
{
    public class Settings : GlobalSettings
    {
        [CommandOption("--groups <GROUPS>")]
        [Description("Name of the log group for stream completion")]
        public virtual string? GroupName { get; init; }
        
        [CommandOption("--type <TYPE>")]
        [Description("Type of completion (groups, streams, metadata)")]
        public virtual string? Type { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (settings.Type == "metadata")
            {
                PrintMetadata(CommandNames.Groups, typeof(GroupsCommand));
                PrintMetadata(CommandNames.Streams, typeof(StreamsCommand));
                PrintMetadata(CommandNames.Fetch, typeof(FetchCommand));
                PrintMetadata(CommandNames.Tail, typeof(TailCommand));
                PrintMetadata(CommandNames.Completion, typeof(CompletionCommand));
                
                return 0;
            }

            using var client = settings.CreateClient();

            if (settings.Type == CommandNames.Groups)
            {
                var response = await client.DescribeLogGroupsAsync(new Amazon.CloudWatchLogs.Model.DescribeLogGroupsRequest(), cancellationToken);
                foreach (var group in response.LogGroups)
                {
                    Console.WriteLine(group.LogGroupName);
                }
            }
            else if (settings.Type == CommandNames.Streams && !string.IsNullOrEmpty(settings.GroupName))
            {
                var response = await client.DescribeLogStreamsAsync(new Amazon.CloudWatchLogs.Model.DescribeLogStreamsRequest
                                                                    {
                                                                        LogGroupName = settings.GroupName,
                                                                        OrderBy = OrderBy.LastEventTime,
                                                                        Descending = true,
                                                                        Limit = 50
                                                                    }, cancellationToken);
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

    private void PrintMetadata(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] System.Type type)
    {
        var options = ReflectionUtils.GetCommandOptions(type);
        options.Add("-h");
        options.Add("--help");
        Console.WriteLine($"COMMAND:{name}:{string.Join(",", options.Distinct())}");
    }
}
