namespace cwlogs.Base;

public class LogEventOutput
{
    public string? Timestamp { get; set; }
    public string? Stream { get; set; }
    public object? Message { get; set; }
    public string? EventId { get; set; }
}
