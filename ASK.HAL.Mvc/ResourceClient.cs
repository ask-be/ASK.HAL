// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using ASK.HAL.Serialization.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASK.HAL.Mvc;

internal class ResourceClient : IResourceClient
{
    public const string ResourceAccessorHttpClientName = "ResourceAccessor";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<JsonOptions> _jsonOptions;

    public ResourceClient(IHttpClientFactory httpClientFactory, IOptions<JsonOptions> jsonOptions)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// Retrieve a Resource at the given Uri.
    /// The current Cookies and Authentication Header will be transferred to the request 
    /// </summary>
    /// <param name="uri">Uri of the resource to retrieve</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The resource</returns>
    public async Task<Resource?> GetResource(
        Uri uri, 
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var client = _httpClientFactory.CreateClient(ResourceAccessorHttpClientName);
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = uri;

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await ResourceJsonSerializer.DeserializeAsync(stream, _jsonOptions.Value.JsonSerializerOptions);
    }
}