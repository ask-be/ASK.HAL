// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using System.Text;
using System.Text.Json;
using ASK.HAL.Serialization.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace ASK.HAL.Mvc.Formatters;

public class JsonResourceInputFormatter : TextInputFormatter
{
    public JsonResourceInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(Constants.HypertextApplicationLanguageJsonMediaType));
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanReadType(Type type) => type == typeof(Resource);

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        var options = context.HttpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value.JsonSerializerOptions 
            ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);

        return await InputFormatterResult.SuccessAsync(
            await ResourceJsonSerializer.DeserializeAsync(
                context.HttpContext.Request.Body,
                options));
    }
}