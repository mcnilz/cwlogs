using System.Text.Json;
using System.Text.Json.Serialization;
using cwlogs.Base;

namespace cwlogs.util;

[JsonSerializable(typeof(LogEventOutput))]
[JsonSerializable(typeof(JsonElement))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
