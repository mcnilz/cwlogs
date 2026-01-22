using System.ComponentModel;
using Amazon.CloudWatchLogs;
using cwlogs.Base;
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
        [Description("Name of the log group for stream completion")]
        public string? GroupName { get; init; }
        
        [CommandOption("--type <TYPE>")]
        [Description("Type of completion (groups, streams, metadata)")]
        public string? Type { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (settings.Type == "metadata")
            {
                // Da wir das Model nicht direkt über den Context bekommen können (Einschränkungen der SDK-Version/Compiler),
                // nutzen wir eine Liste der Commands und deren Optionen, die wir über die Typen der Commands ermitteln.
                
                var commandTypes = new Dictionary<string, System.Type>
                {
                    { CommandNames.Groups, typeof(GroupsCommand) },
                    { CommandNames.Streams, typeof(StreamsCommand) },
                    { CommandNames.Fetch, typeof(FetchCommand) },
                    { CommandNames.Tail, typeof(TailCommand) },
                    { CommandNames.Completion, typeof(CompletionCommand) }
                };

                foreach (var kvp in commandTypes)
                {
                    var name = kvp.Key;
                    var type = kvp.Value;
                    var optionList = new List<string>();
                    
                    // Wir suchen nach dem Settings-Typ. 
                    // AsyncCommand<TSettings> oder Command<TSettings>
                    var currentType = type;
                    System.Type? settingsType = null;
                    while (currentType != null && currentType != typeof(object))
                    {
                        if (currentType.IsGenericType && (currentType.GetGenericTypeDefinition().Name.Contains("AsyncCommand") || currentType.GetGenericTypeDefinition().Name.Contains("Command")))
                        {
                            settingsType = currentType.GetGenericArguments().FirstOrDefault();
                            if (settingsType != null) break;
                        }
                        currentType = currentType.BaseType;
                    }

                    if (settingsType != null)
                    {
                        var currentSettingsType = settingsType;
                        while (currentSettingsType != null && currentSettingsType != typeof(object))
                        {
                            foreach (var prop in currentSettingsType.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly))
                            {
                                // Wir nutzen dynamische Typprüfung, um das Attribut zu finden, 
                                // da wir den Typen selbst (CommandOptionAttribute) nicht direkt nutzen können 
                                // für den Vergleich in manchen Kontexten.
                                foreach (var attr in prop.GetCustomAttributes(true))
                                {
                                    if (attr.GetType().Name == "CommandOptionAttribute")
                                    {
                                        var shorts = attr.GetType().GetProperty("ShortNames")?.GetValue(attr) as System.Collections.IEnumerable;
                                        var longs = attr.GetType().GetProperty("LongNames")?.GetValue(attr) as System.Collections.IEnumerable;
                                        if (shorts != null)
                                        {
                                            foreach (var s in shorts) if (s != null) optionList.Add("-" + s);
                                        }
                                        if (longs != null)
                                        {
                                            foreach (var l in longs) if (l != null) optionList.Add("--" + l);
                                        }
                                    }
                                }
                            }
                            currentSettingsType = currentSettingsType.BaseType;
                        }
                    }
                    
                    // Hilfe-Optionen hinzufügen (Standard in Spectre.Console)
                    optionList.Add("-h");
                    optionList.Add("--help");
                    
                    Console.WriteLine($"COMMAND:{name}:{string.Join(",", optionList.Distinct())}");
                }
                return 0;
            }

            using var client = settings.CreateClient();

            if (settings.Type == CommandNames.Groups)
            {
                var response = await client.DescribeLogGroupsAsync(new Amazon.CloudWatchLogs.Model.DescribeLogGroupsRequest());
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
