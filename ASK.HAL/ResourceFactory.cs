using System.Text.Json;

namespace ASK.HAL;

public class ResourceFactory : IResourceFactory
{
    public ResourceFactory(JsonSerializerOptions options)
    {
        JsonSerializerOptions = options;
    }

    public JsonSerializerOptions JsonSerializerOptions { get; }

    public Resource Create(string self)
    {
        return Create(new Uri(self));
    }
    
    public Resource Create(Uri self)
    {
        return new Resource(JsonSerializerOptions, self);
    }
}