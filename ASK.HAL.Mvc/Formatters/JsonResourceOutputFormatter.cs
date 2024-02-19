using System.Text;
using System.Text.Json;
using ASK.HAL.Serialization.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace ASK.HAL.Mvc.Formatters;

public class JsonResourceOutputFormatter : TextOutputFormatter
{
    public JsonResourceOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(Constants.HypertextApplicationLanguageJsonMediaType));
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type)
    {
        return typeof(Resource).IsAssignableFrom(type);
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        if (context.Object != null)
        {
            var options = context.HttpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value.JsonSerializerOptions 
                ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var result = selectedEncoding.GetBytes(ResourceJsonSerializer.Serialize((Resource) context.Object, options));
            
            context.HttpContext.Response.ContentLength = result.Length;
            await context.HttpContext.Response.BodyWriter.WriteAsync(result);
            await context.HttpContext.Response.BodyWriter.CompleteAsync();
        }
    }
}