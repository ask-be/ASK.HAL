# ASK.HAL

According to [Roy Fielding](https://en.wikipedia.org/wiki/Roy_Fielding), [you may call your API a REST API only if you make use of hypertext](https://roy.gbiv.com/untangled/2008/rest-apis-must-be-hypertext-driven)

The ASK.HAL project contains components to ease the creation of REST API following the best practices described in the
[Hypertext Application Language](https://datatracker.ietf.org/doc/html/draft-kelly-json-hal-11) specification and in books like ["Rest In Practice"](https://www.amazon.com/gp/product/0596805829?ie=UTF8&tag=martinfowlerc-20&linkCode=as2&camp=1789&creative=9325&creativeASIN=0596805829)
and ["REST API Design Cookbook"](https://www.amazon.com/REST-Design-Rulebook-Mark-Masse/dp/1449310508/).

The project is composed of 2 components
* ```ASK.HAL``` project contains base classes and interfaces, including Json serialization
* ```ASK.HAL.Mvc``` project contains component to ease integration of ASK.HAL into ASPNET projects.

## Quick example to create a Resource :

```csharp

using ASK.HAL;
using ASK.HAL.Serialization.Json;

var factory = new ResourceFactory(options);

var author = factory.Create(new Uri("http://example.com/api/authors/12345"))
     .Add(new
     {
         Name = "Marcel Proust",
         BirthDate = new DateTime(1871, 7, 10)
     });

var result = factory.Create(new Uri("http://example.com/api/books/in-search-of-lost-time"))
     .Add(new {Title = "In Search of Lost Time"})
     .AddEmbedded("author",author);

var json = await ResourceJsonSerializer.Serialize(result);
```
Will return the following Json
```json
{
   "_links": {
     "self": {
       "href": "http://example.com/api/book/in-search-of-lost-time"
     }
   },
   "_embedded": {
      "author": {
         "_links": {
           "self": {
             "href": "http://example.com/api/users/12345"
           }
         },
         "name" : "Marcel Proust",
         "birthdate": "1871-07-10T00:00:00.00"
      }
   },
   "title": "In Search of Lost Time"
}
```

## How to integrate into ASP.NET project


### First add the reference to your project
```
Install-Package ASK.HAL
Install-Package ASK.HAL.Mvc
```

### Update your Program.cs file

```csharp

var builder = WebApplication.CreateBuilder(args);

// Add Hypertext Application Language services in Dependency Injection
builder.Services.AddHypertextApplicationLanguage();

// Add services to the container.
builder.Services
       .AddControllers(x =>
       {
           // Add Hypertext Formatter to support application/hal+json Content type
           x.AddHypertextApplicationLanguageFormatters();
       })
       .AddJsonOptions(x =>
       {
           x.AddHypertextApplicationLanguageJsonConverter();
           x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
       });

var app = builder.Build();

...
// Adding HeaderPropagation is required by the IResourceAccessor
app.UseHeaderPropagation();

...
}


```

#### Example of Controller

```csharp
class  MyController : Controller
{
    private IResourceFactory _factory;
    
    // Inject Resource Factory 
    public MyController(IResourceFactory resourceFactory){
        _factory = resourceFactory;
    }
    
    [HttpGet("/{id}",Name = "getbyid")]
    public ActionResult<IResource> Get(int id, [FromQuery] ResourceRequest request)
    {
        var domainEntity = GetDomainEntity();
        
        var result = _resourceFactory.Create(Url.LinkUri("getbyid", new {id = sampleValue.Id}))
            .AddLink("somelink", new Link(Url.LinkUri("getallvalues")))
            .Add(sampleValue);
         
        return Ok(result);
    }
}
```
For more information, look at the sample project

## IResourceClient
If you need to retrieve a remote resource in your code, you can resolve the ```IResourceClient``` from Dependency Injection.
```csharp
public interface IResourceClient{
    Task<Resource?> GetResource(Uri uri,CancellationToken cancellationToken = new CancellationToken());
}
```
> [!IMPORTANT]
> IResourceClient will propagate automatically the current request Cookie and Authentication Http Headers.

## HyperText Cache Pattern

The [Hypertext Cache Pattern](https://datatracker.ietf.org/doc/html/draft-kelly-json-hal#name-hypertext-cache-pattern) allows the server to automatically fetch resource links and add the result as an embedded resource.

Example:

From this request retrieving a book
```http request
GET http://server/api/books/the-way-of-zen
Accept: application/hal+json

{
   "_links": {
      "self": { "href": "http://server/api/books/the-way-of-zen" },
      "author": { "href": "http://server/api/people/alan-watts" }
   }
   "title": "The way of Zen"
}
```
We can ask the server to fetch the Author link as an embedded resource using the "expand" parameter.
```http request
GET http://server/api/books/the-way-of-zen?expand=author
Accept: application/hal+json

{
   "_links": {
      "self": { "href": "http://server/api/books/the-way-of-zen" },
      "author": { "href": "http://server/api/people/alan-watts" }
   }
   "_embedded": {
     "author": {
       "_links": { "self": { "href": "http://server/api/people/alan-watts" } },
       "name": "Alan Watts",
       "born": "January 6, 1915",
       "died": "November 16, 1973"
     }
   }
   "title": "The way of Zen"
}
```
The returned resource will automatically contains the embedded resource without changing anything in the Controller.

### Enable AutoExpand

```csharp
builder.Services
       .AddControllers(x =>
       {
           // Add Hypertext Formatter to support application/hal+json Content type
           x.AddHypertextApplicationLanguageFormatters();
           
           // Add AutoExpand Filter
           x.AddHypertextAutoExpand();
       })
```

> [!TIP]
> As the AutoExpand ActionFilter retrieve the resources using HTTP GET calls, it may be more efficient to let the controller
> retrieve the resource if it can be retrieve locally (in the same controller or in another Controller). Therefore, if the controller process the "expand" parameter
> and add the corresponding embedded resource, the ActionFilter will not process it again.

## Delimited values parameters
ASP.NET Core MVC does not support delimited values for query string parameters because it is not standard.

For example, the uri http://someuri/api?param=val1,val2,val3 cannot be mapped as a string[] in your request object.
(Official workaround: repeat the attribute name "?param=val1&param=val2...")

To support the delimited values parameters, you can register the following ProviderFactory

```csharp
builder.Services
       .AddControllers(x =>
       {
           // Add Support for comma separated values mapped as array 
           x.ValueProviderFactories.AddDelimitedValueProviderFactory(',');
       }
```
