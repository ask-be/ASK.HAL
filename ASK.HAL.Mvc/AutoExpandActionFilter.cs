// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASK.HAL.Mvc;

// ReSharper disable once ClassNeverInstantiated.Global registered as Filter
public class AutoExpandActionFilter : IAsyncActionFilter
{
    private readonly IResourceClient _resourceClient;

    public AutoExpandActionFilter(IResourceClient resourceClient)
    {
        _resourceClient = resourceClient;
    }

    private sealed record ExpandError(string Name, string Message);
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();
        
        if (!result.HttpContext.Request.Query.TryGetValue("expand", out var expands))
            return;

        if (result.Result is not OkObjectResult {Value: Resource resource})
            return;
        
        foreach (var toExpand in expands.Where(x => x is not null).OfType<string>())
        {
            // Ignore Self expand
            if(toExpand == Constants.Self)
                continue;
            
            // Check if we must expand
            if (!resource.ContainsLink(toExpand) || resource.ContainsEmbeddedResource(toExpand)) 
                continue;

            try
            {
                var link = resource.GetLink(toExpand)!;
                
                // Ignore invalid content types
                if(!string.IsNullOrEmpty(link.Type) && link.Type != Constants.HypertextApplicationLanguageJsonMediaType)
                    continue;
                
                var r = await _resourceClient.GetResource(link.Href);
                if (r is not null)
                {
                    resource.AddEmbeddedResource(toExpand, r);
                }
            }
            catch (Exception e)
            {
                resource.Add(new
                {
                    _expandErrors = new []{new ExpandError(toExpand,e.Message)}
                });
            }
        }
    }
}