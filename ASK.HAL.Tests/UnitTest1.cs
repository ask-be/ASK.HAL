using System.Text.Json;
using ASK.HAL;
using ASK.HAL.Serialization.Json;

namespace HAL.Tests;

public class UnitTest1
{
    record ss(string coucou, string sub1);
    
    [Fact]
    public void Test1()
    {
        var resourceFactory = new ResourceFactory(new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = {new ResourceJsonConverter()},
            WriteIndented = true
        });
        
        var r = resourceFactory.Create("http://dfdfd");
        r.Add(new {test = "coucou", sub = new {sub1 = "sub1"}});
        r.Add(new {test2 = "coucou2"});
        r.Add(new {TEST2 = "coucou_MAJUSCULE"});
        r.Add(new
        {
            test = "replaced",
            WithSpecialCase = 33,
            sub = new {coucou = "dfdf", SUB1 = "replaced_sub"}
        });
        r.Add(new ss("Hello", "Sub1"), x => new { x.coucou });
        r.AddLink("prev", new Link("http://prev"));
        r.AddLink("next", new Link("http://next"));
        r.Add(new {array = new string[] {"A", "B", "C"}});
        r.Add(new {array = new string[] {"D", "E", "F"}});

        var dd = ResourceJsonSerializer.Serialize(r, resourceFactory.JsonSerializerOptions);
        Assert.NotNull(dd);

        var rrr = ResourceJsonSerializer.Deserialize(dd, resourceFactory.JsonSerializerOptions);
        var ddddd = rrr.GetValue<ss>("sub");
        
        Assert.Equal("replaced",rrr.GetValue<string>("test"));
    }
}