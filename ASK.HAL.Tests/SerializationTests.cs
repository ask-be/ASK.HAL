using System.Text.Json;
using ASK.HAL;
using ASK.HAL.Serialization.Json;
using FluentAssertions;

namespace HAL.Tests;

public class SerializationTests
{
    private JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = {new ResourceJsonConverter()},
        WriteIndented = false,
    };

    private readonly IResourceFactory _resourceFactory;

    public SerializationTests()
    {
        _resourceFactory = new ResourceFactory(_options);
    }
    
    [Fact]
    public void CanSerializeEmptyResource()
    {
        var r = _resourceFactory.Create();
        var json = ResourceJsonSerializer.Serialize(r, _options);
        json.Should().Be("{}");
    }
    
    [Fact]
    public void CanSerializeResourceWithSingleProperty()
    {
        var r = _resourceFactory.Create().Add(new {test=123});
        var json = ResourceJsonSerializer.Serialize(r, _options);
        json.Should().Be("{\"test\":123}");
    }
    
    [Fact]
    public void CanSerializeResourceWithSelfLink()
    {
        var r = _resourceFactory.Create("http://www.ask.be/");
        var json = ResourceJsonSerializer.Serialize(r, _options);
        json.Should().Be("{\"_links\":{\"self\":{\"href\":\"http://www.ask.be/\"}}}");
    }
}