using System.Text.Json;

namespace cwlogs.util;

public static class JsonUtils
{
    public static object ParseMessage(string message)
    {
        var trimmedMessage = message.Trim();
        if ((trimmedMessage.StartsWith('{') && trimmedMessage.EndsWith('}')) || 
            (trimmedMessage.StartsWith('[') && trimmedMessage.EndsWith(']')))
        {
            try
            {
                return JsonSerializer.Deserialize(message, SourceGenerationContext.Default.JsonElement);
            }
            catch
            {
                // Not valid JSON, keep as string
            }
        }
        return message;
    }
}
