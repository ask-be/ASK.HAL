using System.Text.Json;
using ASK.HAL.Serialization.Json;
using FluentAssertions;

namespace HAL.Tests;

public class DeserializationTests
{
    private JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = {new ResourceJsonConverter()},
        WriteIndented = true,
    };
    
    [Fact]
    public void CanDeserializeEmptyJsonObjectAsResource()
    {
        var resource = ResourceJsonSerializer.Deserialize("{}", options);
        resource.Should().NotBeNull();
        resource.Self.Should().BeNull();
    }
    
        
    [Fact]
    public void CannotDeserializeJsonArrayAsResource()
    {
        Assert.Throws<JsonException>(() => ResourceJsonSerializer.Deserialize("[]", options));
    }
    
    [Fact]
    public void CanDeserializeSinglePropertyJsonAsResource()
    {
        var resource = ResourceJsonSerializer.Deserialize("{\"test\":123}", options);
        resource.Should().NotBeNull();
        resource.Self.Should().BeNull();
        resource.GetValue<int>("test").Should().Be(123);
    }
    
    [Fact]
    public void CanDeserializeSelfOnlyResource()
    {
        var resource = ResourceJsonSerializer.Deserialize("{\"_links\":{\"self\":{\"href\":\"http://www.ask.be\"}}}", options);
        resource.Should().NotBeNull();
        resource.Self.Should().Be("http://www.ask.be");
        resource.ContainsLink("self").Should().BeTrue();
        resource.GetLink("self").Should().NotBeNull();
    }
    
    [Fact]
    public void EnsureWeIgnoreLinkAdditionalJsonProperties()
    {
        var json = "{\"_links\":{\"self\":{\"href\":\"http://www.ask.be\",\"should\":\"be ignored\"}}}";
        var rrr = ResourceJsonSerializer.Deserialize(json, options);
        rrr.ContainsLink("self").Should().BeTrue();
    }
    
    [Fact]
    public void EnsureWeCanDeserializeEmbeddedResources()
    {
        var json = "{\"_embedded\":{\"e\":{\"hello\":\"world\",\"should\":true}}}";
        var rrr = ResourceJsonSerializer.Deserialize(json, options);
        rrr.ContainsEmbeddedResource("e").Should().BeTrue();
        var e = rrr.GetEmbeddedResource("e");
        e.GetValue<string>("hello").Should().Be("world");
        e.GetValue<bool>("should").Should().BeTrue();
    }
}