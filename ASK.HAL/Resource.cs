using System.Text.Json;
using System.Text.Json.Nodes;
using ASK.HAL.Tools;

namespace ASK.HAL;

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

    public SingleOrList<Resource>? GetEmbeddedResource(string rel)
    {
        return _embedded.GetValueOrDefault(rel);
    }

    public bool ContainsLink(string name)
    {
        return _links.ContainsKey(name);
    }

    public bool ContainsEmbedded(string name)
    {
        return _embedded.ContainsKey(name);
    }

    public Resource RemoveLink(string name)
    {
        _links.Remove(name);
        return this;
    }

    public Resource RemoveEmbedded(string name)
    {
        _embedded.Remove(name);
        return this;
    }

    public Resource AddLink(string name, Uri url)
    {
        _links.Add(name, new SingleOrList<Link>(new Link(url)));
        return this;
    }

    public Resource AddLink(string name, Link link)
    {
        _links.Add(name, new SingleOrList<Link>(link));
        return this;
    }

    public Resource AddLink(string name, params Link[] links)
    {
        _links.Add(name, new SingleOrList<Link>(links));
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
        return Values.Deserialize<T>();
    }

    public T? GetValue<T>(string propertyName)
    {
        return Values.TryGetPropertyValue(propertyName, out var node) ? node.Deserialize<T>() : default;
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