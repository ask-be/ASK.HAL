using System.Text.Json;
using System.Text.Json.Nodes;
using ASK.HAL.Tools;

namespace ASK.HAL;

/// <summary>
/// A Resource Object represents a resource.
/// </summary>
public class Resource
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Dictionary<string, SingleOrList<Link>> _links;
    private readonly Dictionary<string, SingleOrList<Resource>> _embedded;

    internal Resource(JsonSerializerOptions jsonSerializerOptions, Uri self)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        _links = new Dictionary<string, SingleOrList<Link>>();
        _embedded = new Dictionary<string, SingleOrList<Resource>>();
        _links.Add(Constants.Self, new SingleOrList<Link>(new Link(self)));
    }

    internal Resource(
        JsonSerializerOptions jsonSerializerOptions,
        Dictionary<string, SingleOrList<Link>> links,
        Dictionary<string, SingleOrList<Resource>> embedded,
        JsonObject values)
    {
        if (!links.ContainsKey(Constants.Self))
            throw new ArgumentException("Self link missing");

        _jsonSerializerOptions = jsonSerializerOptions;
        Values = values;
        _links = links;
        _embedded = embedded;
    }

    public Uri Self => _links[Constants.Self].Value.Href;

    public SingleOrList<Link>? GetLink(string rel)
    {
        return _links.GetValueOrDefault(rel);
    }
    
    public IReadOnlyList<Link> GetCuries()
    {
        return _links.GetValueOrDefault(Constants.Curies)?.Values ?? ArraySegment<Link>.Empty;
    }

    public SingleOrList<Resource>? GetEmbeddedResource(string rel)
    {
        return _embedded.GetValueOrDefault(rel);
    }

    public bool ContainsLink(string rel)
    {
        return _links.ContainsKey(rel);
    }

    public bool ContainsEmbedded(string name)
    {
        return _embedded.ContainsKey(name);
    }

    public Resource RemoveLink(string rel)
    {
        if (!ContainsLink(rel))
            throw new ResourceException($"A link with rel '{rel}' does not exists");
        
        _links.Remove(rel);
        return this;
    }

    public Resource RemoveEmbedded(string name)
    {
        _embedded.Remove(name);
        return this;
    }

    public Resource AddLink(string rel, Uri url)
    {
        if (ContainsLink(rel))
            throw new ResourceException($"A link with rel '{rel}' already exists");
        
        _links.Add(rel, new SingleOrList<Link>(new Link(url)));
        return this;
    }

    public Resource AddLink(string rel, Link link)
    {
        _links.Add(rel, new SingleOrList<Link>(link));
        return this;
    }

    public Resource AddLink(string rel, params Link[] links)
    {
        _links.Add(rel, new SingleOrList<Link>(links));
        return this;
    }
    
    public Resource AddCurie(params Link[] links)
    {
        AddLink(Constants.Curies, links);
        return this;
    }
    
    public Resource AddEmbedded(string name, Resource resource)
    {
        _embedded.Add(name, new SingleOrList<Resource>(resource));
        return this;
    }

    public Resource AddEmbedded(string name, params Resource[] resources)
    {
        _embedded.Add(name, new SingleOrList<Resource>(resources));
        return this;
    }

    public Resource Add<T>(T? values, Func<T, object>? onlyFields = null) where T : class
    {
        if (values == null)
            return this;

        var node = JsonSerializer.SerializeToNode(onlyFields != null ? onlyFields(values) : values, _jsonSerializerOptions);

        Values.Merge(node?.AsObject());
        return this;
    }

    public T? As<T>()
    {
        return Values.Deserialize<T>(_jsonSerializerOptions);
    }

    public T? GetValue<T>(string propertyName)
    {
        try
        {
            return Values.TryGetPropertyValue(propertyName, out var node) ? node.Deserialize<T>() : default;
        }
        catch (Exception e)
        {
            throw new ResourceException($"Error while returning resource property '{propertyName}' as a '{typeof(T)}'", e);
        }
    }
    
    internal IReadOnlyDictionary<string, SingleOrList<Link>> GetLinks()
    {
        return _links;
    }

    internal IReadOnlyDictionary<string, SingleOrList<Resource>> GetEmbeddedResources()
    {
        return _embedded;
    }

    internal JsonObject Values { get; } = new(Constants.DefaultJsonNodeOptions);
}