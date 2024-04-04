// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using System.Text.Json;

namespace ASK.HAL;

public class ResourceFactory : IResourceFactory
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ResourceFactory(JsonSerializerOptions options)
    {
        _jsonSerializerOptions = options;
    }
    
    public Resource Create()
    {
        return new Resource(_jsonSerializerOptions);
    }
    
    public Resource Create(string self)
    {
        return Create(new Uri(self));
    }
    
    public Resource Create(Uri self)
    {
        return new Resource(_jsonSerializerOptions).AddLink(Constants.Self, self);
    }
}