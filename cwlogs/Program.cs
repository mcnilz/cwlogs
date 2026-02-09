using System.Diagnostics.CodeAnalysis;
using cwlogs.Base;
using cwlogs.command;
using cwlogs.settings;
using Spectre.Console.Cli;

namespace cwlogs;

internal static class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GroupsCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(StreamsCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FetchCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TailCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CompletionCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CompleteCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CompletionSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FetchSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(GlobalSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CompleteCommand.Settings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ColorInterceptor))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.ExplainCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.VersionCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.XmlDocCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.EmptyCommandSettings", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.OpenCliGeneratorCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, "Spectre.Console.Cli.CommandOptionAttribute", "Spectre.Console.Cli")]
    [UnconditionalSuppressMessage("Trimming", "IL2111", Justification = "CompleteCommand is explicitly preserved via DynamicDependency.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Spectre.Console.Cli relies on reflection, but we use DynamicDependency to preserve necessary types.")]
    [UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "Spectre.Console.Cli internally calls Assembly.Location, which returns empty in single-file apps, but we provide the application name explicitly.")]
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("cwlogs");
            config.SetInterceptor(new ColorInterceptor());
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
    }
}
