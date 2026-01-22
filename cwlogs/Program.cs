using cwlogs.command;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("cwlogs");
    config.AddCommand<GroupsCommand>("groups")
        .WithDescription("Listet Log-Gruppen auf.");

    config.AddCommand<StreamsCommand>("streams")
        .WithDescription("Listet Log-Streams einer Log-Gruppe auf.");

    config.AddCommand<FetchCommand>("fetch")
        .WithDescription("Zeigt Logs einer Log-Gruppe an.");

    config.AddCommand<TailCommand>("tail")
        .WithDescription("Folgt den Logs einer Log-Gruppe live.");

    config.AddCommand<CompletionCommand>("completion")
        .WithDescription("Generiert das PowerShell-Completion-Script.");

    config.AddCommand<CompleteCommand>("_complete")
        .IsHidden();
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return await app.RunAsync(args);