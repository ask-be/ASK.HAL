// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ASK.HAL.Mvc;

internal class ResourceUriFactory : IResourceUriFactory
{
    private readonly LinkGenerator _linkGenerator;
    private readonly HttpContext _httpContext;

    public ResourceUriFactory(LinkGenerator linkGenerator, IHttpContextAccessor accessor)
    {
        _linkGenerator = linkGenerator;
        _httpContext = accessor.HttpContext ?? throw new Exception("HttpContextAccessor not accessible in DI");
    }

    public Uri GetUriByName(string name, object? parameters = null)
    {
        return new Uri(_linkGenerator.GetUriByName(_httpContext, name, parameters) ?? throw new Exception("Invalid Url"));
    }
}