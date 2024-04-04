using System.Text.Json;
using System.Text.Json.Nodes;
using ASK.HAL.Serialization.Json;
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

    internal Resource(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        _links = new Dictionary<string, SingleOrList<Link>>();
        _embedded = new Dictionary<string, SingleOrList<Resource>>();
    }
    
    /// <summary>
    /// Constructor used by Json Serialization
    /// </summary>
    /// <param name="jsonSerializerOptions"></param>
    /// <param name="links"></param>
    /// <param name="embedded"></param>
    /// <param name="values"></param>
    internal Resource(
        JsonSerializerOptions jsonSerializerOptions,
        Dictionary<string, SingleOrList<Link>> links,
        Dictionary<string, SingleOrList<Resource>> embedded,
        JsonObject values)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        Values = values;
        _links = links;
        _embedded = embedded;
    }
    
    /// <summary>
    /// Returns the 'self' link that corresponds with the IANA registered 'self' relation (as defined by [RFC5988]) whose target is the resource's URI.
    /// </summary>
    public Uri? Self => _links.TryGetValue(Constants.Self, out var selfLink) ? selfLink.Value.Href : null;

    /// <summary>
    /// Return a single link by link relation type (as defined by [RFC5988]
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <returns></returns>
    public Link? GetLink(string rel)
    {
        return _links.GetValueOrDefault(rel)?.Value;
    }
    
    /// <summary>
    /// Return a multi-valued link by link relation type (as defined by [RFC5988]
    /// <remarks>You can use the <seealso cref="Link.Name"/> as a second key if specified</remarks>
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <returns></returns>
    public IReadOnlyList<Link> GetLinks(string rel)
    {
        return _links.GetValueOrDefault(rel)?.Values ?? ArraySegment<Link>.Empty;
    }
    
    /// <summary>
    /// HAL establishes a mechanism called "curies" which allows for link relation types that are compact
    /// and more human readable (eg. "acme:widgets"), whilst still offering a way that they MAY be expanded
    /// into a dereferencable URI providing documentation (eg. "https://docs.acme.com/relations/widgets")
    /// To this end, HAL documents have a reserved link relation type called "curies".
    /// <see cref="https://datatracker.ietf.org/doc/html/draft-kelly-json-hal-11#name-hal-curies"/> for more information.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<Link> GetCuries()
    {
        return _links.GetValueOrDefault(Constants.Curies)?.Values ?? ArraySegment<Link>.Empty;
    }

    /// <summary>
    /// Return a single embedded resource by link relation type (as defined by [RFC5988]
    /// <remarks>Embedded Resources MAY be a full, partial, or inconsistent version of the representation served from the target URI.</remarks>
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <returns></returns>
    public Resource? GetEmbeddedResource(string rel)
    {
        return _embedded.GetValueOrDefault(rel)?.Value;
    }
    
    /// <summary>
    /// Return a multi-valued embedded resource by link relation type (as defined by [RFC5988]
    /// <remarks>Embedded Resources MAY be a full, partial, or inconsistent version of the representation served from the target URI.</remarks>
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <returns></returns>
    public IReadOnlyList<Resource> GetEmbeddedResources(string rel)
    {
        return _embedded.GetValueOrDefault(rel)?.Values ?? ArraySegment<Resource>.Empty;
    }

    /// <summary>
    /// Returns true of the Resource contains a link with the link relation types specified in parameter
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <returns></returns>
    public bool ContainsLink(string rel)
    {
        return _links.ContainsKey(rel);
    }

    /// <summary>
    /// Returns true of the Resource contains a embedded resource with the link relation types specified in parameter
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <returns></returns>
    public bool ContainsEmbeddedResource(string rel)
    {
        return _embedded.ContainsKey(rel);
    }

    /// <summary>
    /// Remove a link if it exists based on it's Link relation type.
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <returns>Current resource for chaining</returns>
    public Resource RemoveLink(string rel)
    {
        _links.Remove(rel);
        return this;
    }

    /// <summary>
    /// Remove an embedded resource if it exists based on it's Link relation type.
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <returns>Current resource for chaining</returns>
    public Resource RemoveEmbeddedResource(string rel)
    {
        _embedded.Remove(rel);
        return this;
    }

    /// <summary>
    /// Add a single-value link to the resource.
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <param name="href">Link URI as defined [RFC3986]</param>
    /// <returns>Current resource for chaining</returns>
    /// <exception cref="ResourceException">If a link with same relation type already exists</exception>
    public Resource AddLink(string rel, Uri href)
    {
        if (ContainsLink(rel))
            throw new ResourceException($"A link with relation type '{rel}' already exists");
        
        _links.Add(rel, new SingleOrList<Link>(new Link(href)));
        return this;
    }

    /// <summary>
    /// Add a single-value link to the resource.
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <param name="link">Link</param>
    /// <returns>Current resource for chaining</returns>
    /// <exception cref="ResourceException">If a link with same relation type already exists</exception>
    public Resource AddLink(string rel, Link link)
    {
        if (ContainsLink(rel))
            throw new ResourceException($"A link with relation type '{rel}' already exists");

        _links.Add(rel, new SingleOrList<Link>(link));
        return this;
    }

    /// <summary>
    /// Add a multi-valued link
    /// </summary>
    /// <param name="rel">Link relation type</param>
    /// <param name="links">Links</param>
    /// <returns>Current resource for chaining</returns>
    /// <exception cref="ResourceException">If a link with same relation type already exists</exception>
    public Resource AddLinks(string rel, params Link[] links)
    {
        if (ContainsLink(rel))
            throw new ResourceException($"A link with relation type '{rel}' already exists");
        
        _links.Add(rel, new SingleOrList<Link>(links));
        return this;
    }
    
    /// <summary>
    /// Add Curie links.
    /// </summary>
    /// <param name="links">Links</param>
    /// <returns>Current resource for chaining</returns>
    /// <exception cref="ResourceException">Curie links already exists</exception>
    public Resource AddCuries(params Link[] links)
    {
        AddLinks(Constants.Curies, links);
        return this;
    }
    
    /// <summary>
    /// Add single-valued Embedded Resource
    /// </summary>
    /// <param name="rel">Relation type</param>
    /// <param name="resource">Resource</param>
    /// <returns>Current resource for chaining</returns>
    /// <exception cref="ResourceException">Embedded resource already exists</exception>
    public Resource AddEmbeddedResource(string rel, Resource resource)
    {
        if (ContainsEmbeddedResource(rel))
            throw new ResourceException($"An embedded resource with relation type '{rel}' already exists");

        _embedded.Add(rel, new SingleOrList<Resource>(resource));
        return this;
    }

    /// <summary>
    /// Add multi-valued Embedded Resource
    /// </summary>
    /// <param name="rel">Relation type</param>
    /// <param name="resources">Resources</param>
    /// <returns>Current resource for chaining</returns>
    /// <exception cref="ResourceException">Embedded resource already exists</exception>
    public Resource AddEmbeddedResources(string rel, params Resource[] resources)
    {
        if (ContainsEmbeddedResource(rel))
            throw new ResourceException($"An embedded resource with relation type '{rel}' already exists");
     
        _embedded.Add(rel, new SingleOrList<Resource>(resources));
        return this;
    }

    /// <summary>
    /// Add Properties to this Resource. This method can be called multiple time and the properties will be merged into the resource.
    /// Properties can be of any type, class, arrays,...
    /// Property values can be overwritten with subsequent calls.
    /// <remarks>JsonSerialization options are the one used either from the <see cref="ResourceFactory"/> or the <see cref="ResourceJsonConverter"/></remarks>
    /// </summary>
    /// <param name="values">Class containing property name and values</param>
    /// <param name="onlyFields">Filter to only add some properties of the values class</param>
    /// <typeparam name="T">Any valid class</typeparam>
    /// <returns>Current resource for chaining</returns>
    public Resource Add<T>(T? values, Func<T, object>? onlyFields = null) where T : class
    {
        if (values == null)
            return this;

        var node = JsonSerializer.SerializeToNode(onlyFields != null ? onlyFields(values) : values, _jsonSerializerOptions);

        Values.Merge(node?.AsObject());
        return this;
    }

    /// <summary>
    /// Retrieve the current resource values as an Object.
    /// <remarks>JsonSerialization options are the one used either from the <see cref="ResourceFactory"/> or the <see cref="ResourceJsonConverter"/></remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Deserialized Object</returns>
    /// <exception cref="ResourceException">If an error occured while deserializing Json</exception>
    public T? As<T>()
    {
        try
        {
            return Values.Deserialize<T>(_jsonSerializerOptions);
        }
        catch(Exception e)
        {
            throw new ResourceException($"Error while deserializing resource properties as a '{typeof(T)}'", e);
        }
    }

    /// <summary>
    /// Retrieve a single property of the resource
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ResourceException">If an error occured while deserializing Json property</exception>
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