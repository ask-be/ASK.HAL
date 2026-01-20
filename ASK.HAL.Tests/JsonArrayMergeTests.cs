using System.Text.Json.Nodes;
using ASK.HAL.Tools;

namespace HAL.Tests;

public class JsonObjectMergeTests
{
    [Fact]
    public void Merge_NullSource_DoesNothing()
    {
        var target = new JsonObject { ["a"] = 1 };
        target.Merge(null);
        AssertJson(target, """{"a":1}""");
    }

    [Theory]
    [InlineData("""{"key":42}""")]
    public void Merge_NewProperties_Added(string sourceJson)
    {
        var target = new JsonObject();
        var source = JsonNode.Parse(sourceJson)!.AsObject();
        target.Merge(source);
        AssertJson(target, sourceJson);
    }

    [Fact]
    public void Merge_PropertyNull_Skipped()
    {
        var target = new JsonObject { ["key"] = "exists" };
        var source = JsonNode.Parse(@"{""key"":null}")!.AsObject()!;
        target.Merge(source);
        Assert.Equal("exists", target["key"]!.GetValue<string>()); 
    }

    [Fact]
    public void Merge_ArrayNulls_Skipped()
    {
        var target = JsonNode.Parse(@"{""arr"":[""keep""]}")!.AsObject()!;
        var source = JsonNode.Parse(@"{""arr"":[null,""add""]}")!.AsObject()!;
        target.Merge(source);
        var expected = JsonNode.Parse(@"{""arr"":[""keep"",""add""]}")!;
        Assert.True(JsonNode.DeepEquals(target, expected));  
    }
    
    [Fact]
    public void Merge_NestedObjects_RecursesDeeply()
    {
        var target = JsonNode.Parse(@"{""level1"":{""a"":1,""nested"":{""b"":2}}}")!.AsObject()!;
        var source = JsonNode.Parse(@"{""level1"":{""a"":10,""nested"":{""c"":3},""new"":4}}")!.AsObject()!;
        
        target.Merge(source);

        var expected = JsonNode.Parse(@"{""level1"":{""a"":10,""nested"":{""b"":2,""c"":3},""new"":4}}")!;
        Assert.True(JsonNode.DeepEquals(target, expected));
    }

    [Fact]
    public void Merge_ArrayConcatenation_Works()
    {
        var target = JsonNode.Parse(@"{""arr"":[{""a"":1},{""b"":2}]}")!.AsObject()!;
        var source = JsonNode.Parse(@"{""arr"":[{""a"":10},{""c"":3}]}")!.AsObject()!;
    
        target.Merge(source);

        var expected = JsonNode.Parse(@"{""arr"":[{""a"":1},{""b"":2},{""a"":10},{""c"":3}]}")!;
        Assert.True(JsonNode.DeepEquals(target, expected));
    }

    [Fact]
    public void Merge_ScalarOverridesScalar()
    {
        var target = new JsonObject { ["key"] = "old" };
        var source = JsonNode.Parse(@"{""key"":""new""}")!.AsObject()!;
        target.Merge(source);
        Assert.Equal("new", target["key"]!.GetValue<string>());
    }

    [Fact]
    public void Merge_TypeChange_ScalarToArray()
    {
        var target = new JsonObject { ["key"] = "old" };
        var source = JsonNode.Parse(@"{""key"":[1,2]}")!.AsObject()!;
        target.Merge(source);
        Assert.IsType<JsonArray>(target["key"]);
        var arr = Assert.IsType<JsonArray>(target["key"]);
        Assert.Equal(2, arr.Count);
    }

    private static void AssertJson(JsonObject actual, string expectedJson)
    {
        var expected = JsonNode.Parse(expectedJson)!;
        Assert.True(JsonNode.DeepEquals(actual, expected));
    }
}
