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

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityCore<ApplicationUser>()
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

builder.Services.AddOpenApi();
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
app.MapGet("/weatherforecast", async (IApplicationDbContext db) =>
{
    var product = new Product() { Id = Guid.CreateVersion7().ToString(), Name = "test" + Guid.CreateVersion7().ToString(), Description = "test", Currency = "USD", Price = 100, Quantity = 99, UOM = "EA" };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

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



