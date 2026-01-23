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
                PrintMetadata("GLOBAL", typeof(GroupsCommand), true); // GroupsCommand uses GlobalSettings
                PrintMetadata(CommandNames.Groups, typeof(GroupsCommand));
                PrintMetadata(CommandNames.Streams, typeof(StreamsCommand));
                PrintMetadata(CommandNames.Fetch, typeof(FetchCommand));
                PrintMetadata(CommandNames.Tail, typeof(TailCommand));
                PrintMetadata(CommandNames.Completion, typeof(CompletionCommand));
                
                return 0;
            }

            var dummyMode = Environment.GetEnvironmentVariable("CWLOGS_DUMMY_COMPLETION");
            if (!string.IsNullOrEmpty(dummyMode))
            {
                if (settings.Type == CommandNames.Groups)
                {
                    Console.WriteLine("dummy-group-1");
                    Console.WriteLine("dummy-group-2");
                    Console.WriteLine("test-group-bash");
                    Console.WriteLine("test-group-pwsh");
                }
                else if (settings.Type == CommandNames.Streams && !string.IsNullOrEmpty(settings.GroupName))
                {
                    Console.WriteLine($"stream-1-for-{settings.GroupName}");
                    Console.WriteLine($"stream-2-for-{settings.GroupName}");
                    Console.WriteLine($"latest-stream");
                }
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
        catch (Exception ex)
        {
            return ErrorHandler.HandleException(ex);
        }
    }

    private void PrintMetadata(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] System.Type type, bool globalOnly = false)
    {
        var options = ReflectionUtils.GetCommandOptionsDetailed(type);
        if (!globalOnly)
        {
            options.Add(("-h", true));
            options.Add(("--help", true));
        }
        
        var optionsStrings = options
            .Distinct()
            .Select(o => $"{o.Name}{(o.IsFlag ? ":FLAG" : ":VALUE")}");
            
        Console.WriteLine($"COMMAND:{name}:{string.Join(",", optionsStrings)}");
    }
}
