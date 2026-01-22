using cwlogs.Base;
using cwlogs.command;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("cwlogs");
    config.AddCommand<GroupsCommand>(CommandNames.Groups)
        .WithDescription("Lists log groups.");

    config.AddCommand<StreamsCommand>(CommandNames.Streams)
        .WithDescription("Lists log streams of a log group.");

    config.AddCommand<FetchCommand>(CommandNames.Fetch)
        .WithDescription("Displays logs of a log group.");

    config.AddCommand<TailCommand>(CommandNames.Tail)
        .WithDescription("Follows logs of a log group live.");

    config.AddCommand<CompletionCommand>(CommandNames.Completion)
        .WithDescription("Generates the auto-completion script for various shells (powershell, bash).");

    config.AddCommand<CompleteCommand>(CommandNames.CompleteInternal)
        .IsHidden();
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return await app.RunAsync(args);