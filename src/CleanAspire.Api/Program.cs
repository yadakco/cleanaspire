using System.Text.Json.Serialization;
using CleanAspire.Api;
using CleanAspire.Application;
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Common.Services;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Identities;
using CleanAspire.Infrastructure;
using CleanAspire.Infrastructure.Persistence;
using CleanAspire.Infrastructure.Persistence.Seed;
using CleanAspire.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Mono.TextTemplating;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using CleanAspire.Api.Identity;


var builder = WebApplication.CreateBuilder(args);





builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();
// add a CORS policy for the client
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins("localhost", "https://localhost:7341", "https://localhost:7123")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    options.UseCookieAuthentication();
    options.UseExamples();
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Don't serialize null values
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    // Pretty print JSON
    options.SerializerOptions.WriteIndented = true;
});
builder.Services.AddServiceDiscovery();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();
await app.InitializeDatabaseAsync();
// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.UseCors("wasm");
app.MapGet("/weatherforecast", () =>
{

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
}).WithTags("Weather")
  .WithSummary("Get the weather forecast for the next 5 days.")
  .WithDescription("Returns an array of weather forecast data including the date, temperature, and weather summary for the next 5 days. Each forecast entry provides information about the expected temperature and a brief summary of the weather conditions.");

app.Use(async (context, next) =>
{
    var currentUserContextSetter = context.RequestServices.GetRequiredService<ICurrentUserContextSetter>();
    try
    {
        currentUserContextSetter.SetCurrentUser(context.User);
        await next.Invoke();
    }
    finally
    {
        currentUserContextSetter.Clear();
    }
});

app.MapDefaultEndpoints();
app.MapIdentityApi<ApplicationUser>();
app.MapIdentityApiAdditionalEndpoints<ApplicationUser>();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}



