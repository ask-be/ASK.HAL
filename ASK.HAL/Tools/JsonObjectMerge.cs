using System.Diagnostics;
using System.Text.Json.Nodes;

namespace ASK.HAL.Tools;

public static class JsonObjectMerge
{
    public static void Merge(this JsonObject target, JsonObject? source)
    {
        if(source is null)
            return;
        
        foreach (var sourceProperty in source)
        {
            if(sourceProperty.Value is null)
                continue;
            
            if(target.TryGetPropertyValue(sourceProperty.Key, out var targetProperty) && targetProperty is not null)
            {
                MergeExistingProperty(sourceProperty.Key, targetProperty, sourceProperty.Value);
            }
            else
            {
                target.Add(sourceProperty.Key, DeepClone(sourceProperty.Value));
            }
        }
    }

    private static void MergeExistingProperty(string propertyName,JsonNode targetPropertyValue, JsonNode sourcePropertyValue)
    {
        switch (targetPropertyValue)
        {
            case JsonObject nestedTarget when sourcePropertyValue is JsonObject nestedSource:
                Merge(nestedTarget,nestedSource);
                break;
            case JsonArray arrayTarget when sourcePropertyValue is JsonArray arraySource:
                foreach (var e in arraySource.Where(x => x is not null))
                {
                    Debug.Assert(e != null, nameof(e) + " != null");
                    arrayTarget.Add(DeepClone(e));
                }
                break;
            default:
                ReplaceWith(propertyName, targetPropertyValue,sourcePropertyValue);
                break;
        }
    }

    private static JsonNode DeepClone(JsonNode node)
    {
#if NET7_0_OR_GREATER
        return node.DeepClone();
#else
        return JsonNode.Parse(node.ToJsonString(), Constants.DefaultJsonNodeOptions);
#endif
    }
    
    private static void ReplaceWith(string propertyName,JsonNode target, JsonNode source)
    {
#if NET7_0_OR_GREATER
        target.ReplaceWith(source.DeepClone());
#else
        target.Parent![propertyName] = DeepClone(source);
#endif
    }
}