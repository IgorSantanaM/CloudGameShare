using System.Text.Json.Serialization;
using CloudGameShare.GamingSession.Application;
using CloudGameShare.GamingSession.Infra.Data;
using CloudGameShare.GamingSession.Infra.Services;
using CloudGameShare.GamingSession.Presentation.WebApi.Authentication;
using CloudGameShare.GamingSession.Presentation.WebApi.Endpoints.Internal;
using CloudGameShare.GamingSession.Presentation.WebApi.Middlewares;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

builder.Services.AddSessionAuthentication(builder.Configuration, builder.Environment)
    .AddGamingSessionApplication()
    .AddGamingSessionData(builder.Configuration)
    .AddGamingSessionServices(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Gaming Sessions API v1");
        options.DocumentTitle = "CloudGameShare Gaming Sessions API";
    });
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints<Program>();

app.MapGet("/", () => "Gaming Sessions API is running.");

await app.Services.EnsureGamingSessionIndexesAsync(app.Lifetime.ApplicationStopping);
await app.RunAsync();
