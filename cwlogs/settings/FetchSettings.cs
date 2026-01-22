using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.settings;

public class FetchSettings : StreamsSettings
{
    [CommandOption("-s|--stream <STREAM>")]
    [Description("Name des Log-Streams oder eine Zahl n für die n neuesten Streams.")]
    public string? Stream { get; [UsedImplicitly] init; }

    [CommandOption("-l|--limit <LIMIT>")]
    [DefaultValue(50)]
    [Description("Maximale Anzahl der Log-Einträge.")]
    public int Limit { get; init; } = 50;

    [CommandOption("--sort <SORT>")]
    [DefaultValue("desc")]
    [Description("Sortierreihenfolge (asc, desc).")]
    public string Sort { get; init; } = "desc";

    [CommandOption("--single-line")]
    [Description("Gibt Log-Einträge einzeilig aus.")]
    public bool SingleLine { get; [UsedImplicitly] init; }

    [CommandOption("--raw")]
    [Description("Gibt nur die Log-Nachricht ohne Metadaten aus.")]
    public bool Raw { get; [UsedImplicitly] init; }

    [CommandOption("--clean")]
    [Description("Entfernt AWS Lambda Prefix (Timestamp, RequestID, Level) aus der Nachricht.")]
    public bool Clean { get; [UsedImplicitly] init; }
}