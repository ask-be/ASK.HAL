using System.Net;
using System.Net.Http.Headers;
using ASK.HAL.Mvc.Formatters;
using ASK.HAL.Serialization.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace ASK.HAL.Mvc;

public static class Extensions
{
    /// <summary>
    /// Add JsonConverters to support Resource objects
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static JsonOptions AddHypertextApplicationLanguageJsonConverter(this JsonOptions x)
    {
        x.JsonSerializerOptions.Converters.Add(new ResourceJsonConverter());
        return x;
    }

    /// <summary>
    /// Add Input and Output Formatter to support application/hal+json Content Type
    /// Add 
    /// </summary>
    /// <param name="x">MvcOptions</param>
    /// <param name="enableAutoExpand">True to enable the auto-expand filter</param>
    /// <returns></returns>
    public static MvcOptions AddHypertextApplicationLanguageFormatters(this MvcOptions x, bool enableAutoExpand = true)
    {
        x.InputFormatters.Insert(0,new JsonResourceInputFormatter());
        x.OutputFormatters.Insert(0,new JsonResourceOutputFormatter());
        return x;
    }
    
    public static MvcOptions AddHypertextAutoExpand(this MvcOptions x)
    {
        x.Filters.Add<AutoExpandActionFilter>();
        return x;
    }

    /// <summary>
    /// Add support for Hypertext Application Language Resource factory <see cref="IResourceFactory"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddHypertextApplicationLanguage(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        
        services.AddHeaderPropagation(x =>
        {
            x.Headers.Add(HttpRequestHeader.Authorization.ToString());
            x.Headers.Add(HttpRequestHeader.Cookie.ToString());
        });

        services.AddHttpClient(ResourceClient.ResourceAccessorHttpClientName, x =>
                {
                    x.DefaultRequestHeaders.Accept.Clear();
                    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HypertextApplicationLanguageJsonMediaType));
                })
                .AddHeaderPropagation();
        
        services.AddTransient<IResourceClient, ResourceClient>();
        services.AddTransient<IResourceUriFactory, ResourceUriFactory>();
        services.AddSingleton<IResourceFactory>(x =>
        {
            var cfg = x.GetService<IOptions<JsonOptions>>() ??
                throw new ApplicationException("Unable to resolve IOptions<JsonOptions>");

            return new ResourceFactory(cfg.Value.JsonSerializerOptions);
        });
        return services;
    }

    public static HttpResponse SetAllowHeader(this HttpResponse response, params HttpMethod[] methods)
    {
        response.Headers.AppendCommaSeparatedValues(HeaderNames.Allow,methods.Select(x =>x.Method).ToArray());
        return response;
    }
}