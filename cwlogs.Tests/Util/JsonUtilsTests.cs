using System.Text.Json;
using cwlogs.util;
using FluentAssertions;
using Xunit;

namespace cwlogs.Tests.Util;

public class JsonUtilsTests
{
    [Fact]
    public void ParseMessage_NotJson_ReturnsString()
    {
        var message = "Hello World";
        var result = JsonUtils.ParseMessage(message);
        result.Should().Be(message);
    }

    [Fact]
    public void ParseMessage_ValidJsonObject_ReturnsJsonElement()
    {
        var message = "{\"key\": \"value\"}";
        var result = JsonUtils.ParseMessage(message);
        result.Should().BeOfType<JsonElement>();
        var element = (JsonElement)result;
        element.GetProperty("key").GetString().Should().Be("value");
    }

    [Fact]
    public void ParseMessage_ValidJsonArray_ReturnsJsonElement()
    {
        var message = "[1, 2, 3]";
        var result = JsonUtils.ParseMessage(message);
        result.Should().BeOfType<JsonElement>();
        var element = (JsonElement)result;
        element.GetArrayLength().Should().Be(3);
    }

    [Fact]
    public void ParseMessage_InvalidJsonStartingWithBrace_ReturnsString()
    {
        var message = "{invalid}";
        var result = JsonUtils.ParseMessage(message);
        result.Should().Be(message);
    }
}
