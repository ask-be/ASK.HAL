// SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
// SPDX-License-Identifier: LGPL-3.0-only

using ASK.HAL;
using ASK.HAL.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace HAL.Mvc.Sample.Controllers;

[ApiController]
public class SampleApiController : Controller
{
    private readonly IResourceFactory _resourceFactory;
    private readonly IResourceUriFactory _resourceUriFactory;

    public SampleApiController(IResourceFactory resourceFactory, IResourceUriFactory resourceUriFactory)
    {
        _resourceFactory = resourceFactory;
        _resourceUriFactory = resourceUriFactory;
    }
    
    [HttpOptions]
    [Route("/api/sample")]
    public IActionResult ReturnResourceOptions()
    {
        Response.SetAllowHeader(HttpMethod.Get,HttpMethod.Post,HttpMethod.Delete, HttpMethod.Patch);
        return Ok();
    }
    
    [HttpGet]
    [Route("/api/sample", Name = "test")]
    public IActionResult ReturnResource([FromQuery]string? expand)
    {
        return Ok(_resourceFactory
                  .Create(_resourceUriFactory.GetUriByName("test"))
                  .AddLink("coucou", new Uri("http://coucou.com"))
                  .AddLink("test", new Uri("http://test.local"))
                  .AddLink("loop", _resourceUriFactory.GetUriByName("test"))
                  .AddLink("image", new Link(new Uri("http://logo.com/logo.png"), type: "image/png", title:"Profile Image"))
                  .Add(new
                  {
                      SomeValue = 33,
                      Test = new
                      {
                          SuperProperty = "test\ud83d\ude00"
                      }
                  }));
    }
    
    [HttpGet]
    [Route("/api/list", Name = "list")]
    public IActionResult ReturnResourceList([FromQuery]CollectionRequest request)
    {
        var result = _resourceFactory.Create(_resourceUriFactory.GetUriByName("list"));
        result.Add(request);

        result.AddEmbeddedResources("list",
            Enumerable
                .Range(1, 500)
                .Skip(request.Index)
                .Take(request.Max)
                .Select(x => _resourceFactory.Create(_resourceUriFactory.GetUriByName("test")).Add(new {Counter = x})).ToArray());

        return Ok(result);
    }
    
    [HttpPost]
    [Route("/api/sample")]
    public IActionResult PostResource(Resource resource)
    {
        return Ok(resource.GetValue<int>("SomeValue"));
    }
}