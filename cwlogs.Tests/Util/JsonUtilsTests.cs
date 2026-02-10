using System.Text.Json;
using cwlogs.util;

namespace cwlogs.Tests.Util;

public class JsonUtilsTests
{
    [Fact]
    public void ParseMessage_NotJson_ReturnsString()
    {
        var message = "Hello World";
        var result = JsonUtils.ParseMessage(message);
        Assert.Equal(message, result);
    }

    [Fact]
    public void ParseMessage_ValidJsonObject_ReturnsJsonElement()
    {
        var message = "{\"key\": \"value\"}";
        var result = JsonUtils.ParseMessage(message);
        Assert.IsType<JsonElement>(result);
        var element = (JsonElement)result;
        Assert.Equal("value", element.GetProperty("key").GetString());
    }

    [Fact]
    public void ParseMessage_ValidJsonArray_ReturnsJsonElement()
    {
        var message = "[1, 2, 3]";
        var result = JsonUtils.ParseMessage(message);
        Assert.IsType<JsonElement>(result);
        var element = (JsonElement)result;
        Assert.Equal(3, element.GetArrayLength());
    }

    [Fact]
    public void ParseMessage_InvalidJsonStartingWithBrace_ReturnsString()
    {
        var message = "{invalid}";
        var result = JsonUtils.ParseMessage(message);
        Assert.Equal(message, result);
    }
}
