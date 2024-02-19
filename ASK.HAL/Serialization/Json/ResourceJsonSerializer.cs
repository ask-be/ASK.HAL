using System.Text.Json;

namespace ASK.HAL.Serialization.Json;

public static class ResourceJsonSerializer
{
    public static string Serialize(Resource resource, JsonSerializerOptions options)
    {   
        return JsonSerializer.Serialize(resource,options);
    }
    public static async Task SerializeAsync( Stream stream, Resource resource, JsonSerializerOptions options)
    {
        await JsonSerializer.SerializeAsync<Resource>(stream,resource, options);
    }
    
    public static Resource? Deserialize(string resource, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<Resource>(resource,options);
    }
    public static async Task<Resource?> DeserializeAsync(Stream stream, JsonSerializerOptions options)
    {
        return await JsonSerializer.DeserializeAsync<Resource>(stream,options);
    }
}