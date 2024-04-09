// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

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

        var ddd =
            "{\"_links\":{\"self\":{\"href\":\"http://host.docker.internal:5000/api/projects/d6bf4078dfd048439d5e6f9e61a8d18f/metavault/sourcesystems/b4ffcb5a-f4cb-49a6-a7d3-239876920bad\"},\"project\":{\"href\":\"http://host.docker.internal:5000/api/projects/d6bf4078dfd048439d5e6f9e61a8d18f\"},\"dataPackages\":{\"href\":\"http://host.docker.internal:5000/api/projects/d6bf4078dfd048439d5e6f9e61a8d18f/metavault/sourcesystems/b4ffcb5a-f4cb-49a6-a7d3-239876920bad/datapackages\"},\"image\":{\"href\":\"http://host.docker.internal:5000/api/projects/d6bf4078dfd048439d5e6f9e61a8d18f/metavault/sourcesystems/b4ffcb5a-f4cb-49a6-a7d3-239876920bad/image\"}},\"_embedded\":{\"dataPackages\":[]},\"id\":\"b4ffcb5a-f4cb-49a6-a7d3-239876920bad\",\"qualityType\":\"None\",\"qualityTypeString\":\"None\",\"name\":\"Sales Domain\",\"code\":\"SALES_DOMAIN\",\"dataPackages\":{},\"packages\":[]}";
    }

    [Fact]
    public void EnsureWeCanDeserializeEmptyEmbeddedResources()
    {
        var json = "{\"_embedded\":{\"emptyEmbedded\":[]}}";
        var rrr = ResourceJsonSerializer.Deserialize(json, options);
        rrr.ContainsEmbeddedResource("emptyEmbedded").Should().BeTrue();
        rrr.GetEmbeddedResources("emptyEmbedded").Count.Should().Be(0);
    }
    [Fact]
    public void EnsureWeCanDeserializeEmptyLinkResources()
    {
        var json = "{\"_links\":{\"testEmptyLinks\":[]}}";
        var rrr = ResourceJsonSerializer.Deserialize(json, options);
        rrr.ContainsLink("testEmptyLinks").Should().BeTrue();
        rrr.GetLinks("testEmptyLinks").Count.Should().Be(0);
    }

    
}