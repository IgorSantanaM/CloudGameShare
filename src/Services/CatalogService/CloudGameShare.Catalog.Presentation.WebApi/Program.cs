using CloudGameShare.Catalog.Application;
using CloudGameShare.Catalog.Infra.Data;
using CloudGameShare.Catalog.Infra.Data.Contexts;
using CloudGameShare.Catalog.Presentation.WebApi.Endpoints.Internal;
using CloudGameShare.Catalog.Presentation.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Catalog")
    ?? throw new InvalidOperationException("Configure the Catalog connection string.");

builder.Services.AddOpenApi(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);

builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCatalogApplication();

builder.Services.AddCatalogData(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Catalog API v1");
        options.DocumentTitle = "CloudGameShare Catalog API";
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseEndpoints<Program>();

app.MapGet("/", () => "Catalog API is running.");

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CloudGameShareCatalogContext>();
    await context.Database.MigrateAsync();
}

await app.RunAsync();
