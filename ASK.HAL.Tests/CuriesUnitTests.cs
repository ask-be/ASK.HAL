using System.Text.Json;
using ASK.HAL;
using ASK.HAL.Serialization.Json;
using FluentAssertions;

namespace HAL.Tests;

public class CuriesUnitTests
{
    private readonly IResourceFactory _resourceFactory = new ResourceFactory(new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = {new ResourceJsonConverter()},
        WriteIndented = true,
    });

    [Fact]
    public void CanCreateSimpleCurie()
    {
        var r = _resourceFactory.Create("/orders")
                                .AddCuries(new Link("https://docs.acme.com/relations/{rel}", name: "acme", templated: true))
                                .AddLink("acme:widgets", new Link("/widgets"));

        r.GetCuries().Count.Should().Be(1);
        r.GetCuries()[0].Href.Should().Be("https://docs.acme.com/relations/{rel}");
        r.GetCuries()[0].Name.Should().Be("acme");
        r.GetCuries()[0].Templated = true;
        r.GetLink("acme:widgets").Should().NotBeNull();
    }
    
    [Fact]
    public void CanCreateCurieForVersionedLinkRelationType()
    {
        var r = _resourceFactory.Create("/")
                                .AddCuries(
                                    new Link("https://docs.example.com/relations/v1/{rel}", name: "v1", templated: true),
                                    new Link("https://docs.example.com/relations/v2/{rel}", name: "v2", templated: true))
                                .AddLink("v1:orders", new Link("https://api.example.com/orders", deprecation: "https://dev.example.com/deprecations/v1-orders"))
                                .AddLink("v2:orders", new Link("https://api.example.com/order-list"));

        r.GetCuries().Count.Should().Be(2);
        r.GetCuries()[0].Href.Should().Be("https://docs.example.com/relations/v1/{rel}");
        r.GetCuries()[0].Name.Should().Be("v1");
        r.GetCuries()[0].Templated = true;
        r.GetCuries()[1].Href.Should().Be("https://docs.example.com/relations/v2/{rel}");
        r.GetCuries()[1].Name.Should().Be("v2");
        r.GetCuries()[1].Templated = true;
        
        r.GetLink("v1:orders").Should().NotBeNull();
        r.GetLink("v2:orders").Should().NotBeNull();
    }
}