// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Nodes;

namespace ASK.HAL.Tools;

public static class JsonObjectMerge
{
    public static void Merge(this JsonObject target, JsonObject? source)
    {
        if (source is null) return;

        foreach (var (key, sourceValue) in source)
        {
            if (sourceValue is null) continue;

            if (target.TryGetPropertyValue(key, out var targetValue) && targetValue is not null)
            {
                MergeProperty(targetValue, sourceValue);
            }
            else
            {
                target[key] = sourceValue.DeepClone();
            }
        }
    }

    private static void MergeProperty(JsonNode targetPropertyValue, JsonNode sourcePropertyValue)
    {
        switch (targetPropertyValue, sourcePropertyValue)
        {
            case (JsonObject tObj, JsonObject sObj):
                tObj.Merge(sObj);
                break;
            case (JsonArray tArr, JsonArray sArr):
                foreach (var e in sArr)
                {
                    if (e is null)
                        continue;
                    
                    tArr.Add(e.DeepClone());
                }
                break;
            default:
                targetPropertyValue.ReplaceWith(sourcePropertyValue.DeepClone());
                break;
        }
    }
}