using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class GroupsCommand : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = settings.CreateClient();
            var response =
                await client.DescribeLogGroupsAsync(new Amazon.CloudWatchLogs.Model.DescribeLogGroupsRequest(),
                    cancellationToken);

            var table = new Table().NoBorder();
            table.AddColumn("Log Group Name");
            table.AddColumn("Creation Time");

            foreach (var group in response.LogGroups)
            {
                var creationTime = group.CreationTime.ToString();
                table.AddRow(Markup.Escape(group.LogGroupName ?? string.Empty),
                    Markup.Escape(creationTime ?? string.Empty));
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