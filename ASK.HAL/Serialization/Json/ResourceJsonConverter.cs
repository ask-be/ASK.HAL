// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ASK.HAL.Serialization.Json;

public class ResourceJsonConverter : JsonConverter<Resource>
{
    private delegate T DeserializeFunc<out T>(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
    
    public override Resource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DeserializeResource(ref reader, options);
    }

    private Resource DeserializeResource(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
        
        var values = new JsonObject(new JsonNodeOptions{PropertyNameCaseInsensitive = true});
        var links = new Dictionary<string, SingleOrList<Link>>();
        var embedded = new Dictionary<string, SingleOrList<Resource>>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            var propertyName = ReadPropertyName(ref reader);

            switch (propertyName)
            {
                case Constants.Links:
                    foreach (var l in ReadSingleOrList(ref reader, options, DeserializeLink))
                    {
                        links.Add(l.Key, l.Value);
                    }
                    break;
                case Constants.Embedded:
                    foreach (var l in ReadSingleOrList(ref reader, options, Read))
                    {
                        embedded.Add(l.Key, l.Value);
                    }
                    break;
                default:
                    values.Add(propertyName, JsonNode.Parse(ref reader));
                    break;
            }
        }

        return new Resource(options, links,embedded,values);
    }
    
    private static Dictionary<string, SingleOrList<T>> ReadSingleOrList<T>(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options,
        DeserializeFunc<T> objReader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var result = new Dictionary<string, SingleOrList<T>>();
           
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
                
            var propertyName = ReadPropertyName(ref reader);

            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray:
                {
                    var items = new List<T>();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                        {
                            result.Add(propertyName,new SingleOrList<T>(items));
                            break;
                        }
                        items.Add(objReader(ref reader,typeof(T), options));
                    }

                    break;
                }
                case JsonTokenType.StartObject:
                    result.Add(propertyName,new SingleOrList<T>(objReader(ref reader,typeof(T), options)));
                    break;
                case JsonTokenType.None:
                    break;
                case JsonTokenType.EndObject:
                    break;
                case JsonTokenType.EndArray:
                    break;
                case JsonTokenType.PropertyName:
                    break;
                case JsonTokenType.Comment:
                    break;
                case JsonTokenType.String:
                    break;
                case JsonTokenType.Number:
                    break;
                case JsonTokenType.True:
                    break;
                case JsonTokenType.False:
                    break;
                case JsonTokenType.Null:
                    break;
                default:
                    throw new JsonException();
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, Resource value, JsonSerializerOptions options)
    {
        SerializeResource(value, writer, options);
    }
    
    private static void SerializeResource(Resource resource, Utf8JsonWriter writer, JsonSerializerOptions o)
    {
        writer.WriteStartObject();
        SerializeLinks(resource, writer, o);
        SerializeEmbeddedResources(resource, writer, o);
        foreach (var p in resource.Values.Where(x => x.Value is not null))
        {
            WritePropertyName(writer, p.Key, o);
            p.Value!.WriteTo(writer, o);
        }
        writer.WriteEndObject();
    }

    private static void SerializeLinks(Resource resource, Utf8JsonWriter writer, JsonSerializerOptions o)
    {
        var allLinks = resource.GetLinks();
        
        if (allLinks.Count == 0)
            return;
        
        writer.WritePropertyName(Constants.Links);
        writer.WriteStartObject();
        foreach (var (key, links) in allLinks)
        {
            WritePropertyName(writer, key, o);
            if (links.SingleValued)
            {
                SerializeLink(writer, links.Value, o);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var link in links.Values)
                {
                    SerializeLink(writer, link, o);
                }
                writer.WriteEndArray();
            }
        }
        writer.WriteEndObject();
    }
    
    private static void SerializeEmbeddedResources(Resource resource, Utf8JsonWriter writer, JsonSerializerOptions o)
    {
        if(resource.GetEmbeddedResources().Count == 0)
            return;
        
        writer.WritePropertyName(Constants.Embedded);
        writer.WriteStartObject();
        foreach (var (key, embedded) in resource.GetEmbeddedResources())
        {
            WritePropertyName(writer, key, o);
            if (embedded.SingleValued)
            {
                SerializeResource(embedded.Value, writer,  o);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var e in embedded.Values)
                {
                    SerializeResource(e, writer, o);
                }
                writer.WriteEndArray();
            }
        }
        writer.WriteEndObject();
    }

    private static Link DeserializeLink(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        Uri? href = null;
        string? title = null;
        var templated = false;
        string? deprecation = null;
        string? name = null;
        string? profile = null;
        string? type = null;
        string? hreflang = null;
            
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (href == null)
                    throw new JsonException("Link must have an href");

                return new Link(href, title, type, name, templated, deprecation, hreflang, profile);
            }
            
            var property = ReadPropertyName(ref reader);
            switch (property)
            {
                case Constants.HrefPropertyName:
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        href = new Uri(reader.GetString()!);
                    }
                    break;
                case Constants.DeprecationPropertyName:
                    deprecation = reader.GetString();
                    break;
                case Constants.LangPropertyName:
                    hreflang = reader.GetString();
                    break;
                case Constants.NamePropertyName:
                    name = reader.GetString();
                    break;
                case Constants.ProfilePropertyName:
                    profile = reader.GetString();
                    break;
                case Constants.TemplatedPropertyName:
                    templated = reader.GetBoolean();
                    break;
                case Constants.TitlePropertyName:
                    title = reader.GetString();
                    break;
                case Constants.TypePropertyName:
                    type = reader.GetString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException();
    }
    
    private static void SerializeLink(Utf8JsonWriter writer, Link link, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        WritePropertyName(writer, Constants.HrefPropertyName, options);
        writer.WriteStringValue(link.Href.ToString());
        if (!string.IsNullOrWhiteSpace(link.Deprecation))
        {
            writer.WriteString(Constants.DeprecationPropertyName,link.Deprecation);
        }

        if (!string.IsNullOrWhiteSpace(link.Hreflang))
        {
            writer.WriteString(Constants.LangPropertyName,link.Hreflang);
        }

        if (!string.IsNullOrWhiteSpace(link.Name))
        {
            writer.WriteString(Constants.NamePropertyName,link.Name);
        }

        if (!string.IsNullOrWhiteSpace(link.Profile))
        {
            writer.WriteString(Constants.ProfilePropertyName,link.Profile);
        }

        if (link.Templated.HasValue && link.Templated.Value)
        {
            writer.WriteBoolean(Constants.TemplatedPropertyName,true);
        }

        if (!string.IsNullOrWhiteSpace(link.Title))
        {
            writer.WriteString(Constants.TitlePropertyName,link.Title);
        }

        if (!string.IsNullOrWhiteSpace(link.Type))
        {
            writer.WriteString(Constants.TypePropertyName,link.Type);
        }
        
        writer.WriteEndObject();
    }

    private static string ReadPropertyName(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        var propertyName = reader.GetString() ?? throw new InvalidOperationException("Unable to read property Value");
        reader.Read();
        return propertyName;
    }
    
    private static void WritePropertyName(Utf8JsonWriter writer, string propertyName, JsonSerializerOptions options)
    {
        writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
    }
    
}