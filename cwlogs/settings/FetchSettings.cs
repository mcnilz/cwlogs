using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.settings;

public class FetchSettings : StreamsSettings
{
    [CommandOption("-s|--stream <STREAM>")]
    [Description("Name of the log stream or a number n for the n latest streams.")]
    public virtual string? Stream { get; [UsedImplicitly] init; }

    [CommandOption("-l|--limit <LIMIT>")]
    [DefaultValue(50)]
    [Description("Maximum number of log entries.")]
    public virtual int Limit { get; init; } = 50;

    [CommandOption("--sort <SORT>")]
    [DefaultValue("asc")]
    [Description("Sort order (asc, desc).")]
    public virtual string Sort { get; init; } = "asc";

    [CommandOption("--single-line")]
    [Description("Outputs log entries on a single line.")]
    public virtual bool SingleLine { get; [UsedImplicitly] init; }

    [CommandOption("--raw")]
    [Description("Outputs only the log message without metadata.")]
    public virtual bool Raw { get; [UsedImplicitly] init; }

    [CommandOption("--clean")]
    [Description("Removes AWS Lambda prefix (Timestamp, RequestID, Level) from the message.")]
    public virtual bool Clean { get; [UsedImplicitly] init; }

    [CommandOption("--json")]
    [Description("Outputs log entries as JSON objects.")]
    public virtual bool Json { get; [UsedImplicitly] init; }

    [CommandOption("--since <SINCE>")]
    [Description("Only show logs after a specific time (e.g., '1h', '30m', '2025-01-22').")]
    public virtual string? Since { get; [UsedImplicitly] init; }
}