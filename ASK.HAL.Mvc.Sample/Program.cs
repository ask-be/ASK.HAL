using System.Text.Json;
using ASK.HAL.Mvc;
using ASK.HAL.Mvc.DelimitedQueryString;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
       .AddControllers(x =>
       {
           // Add Hypertext Formatter to support application/hal+json Content type
           x.AddHypertextApplicationLanguageFormatters();
           
           // Add AutoExpand Filter
           x.AddHypertextAutoExpand();
           
           // Add Support for comma separated values mapped as array 
           x.ValueProviderFactories.AddDelimitedValueProviderFactory(',');
           
           x.RespectBrowserAcceptHeader = true;
           x.ReturnHttpNotAcceptable = true;
       })
       .AddJsonOptions(x =>
       {
           x.AddHypertextApplicationLanguageJsonConverter();
           x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
       });

// Add Hypertext Application Language
builder.Services.AddHypertextApplicationLanguage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseHeaderPropagation();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();