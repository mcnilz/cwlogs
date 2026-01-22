using cwlogs.Base;
using cwlogs.command;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("cwlogs");
    config.AddCommand<GroupsCommand>(CommandNames.Groups)
        .WithDescription("Listet Log-Gruppen auf.");

    config.AddCommand<StreamsCommand>(CommandNames.Streams)
        .WithDescription("Listet Log-Streams einer Log-Gruppe auf.");

    config.AddCommand<FetchCommand>(CommandNames.Fetch)
        .WithDescription("Zeigt Logs einer Log-Gruppe an.");

    config.AddCommand<TailCommand>(CommandNames.Tail)
        .WithDescription("Folgt den Logs einer Log-Gruppe live.");

    config.AddCommand<CompletionCommand>(CommandNames.Completion)
        .WithDescription("Generiert das PowerShell-Completion-Script.");

    config.AddCommand<CompleteCommand>(CommandNames.CompleteInternal)
        .IsHidden();
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return await app.RunAsync(args);