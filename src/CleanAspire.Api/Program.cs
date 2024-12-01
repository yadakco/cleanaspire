using System.Text.Json.Serialization;
using CleanAspire.Api;
using CleanAspire.Application;
using CleanAspire.Application.Common.Services;
using CleanAspire.Domain.Identities;
using CleanAspire.Infrastructure;
using CleanAspire.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using Microsoft.OpenApi;
using CleanAspire.Api.Identity;
using Microsoft.Extensions.FileProviders;
using CleanAspire.Api.Endpoints;


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
var allowedCorsOrigins = builder.Configuration.GetValue<string>("AllowedCorsOrigins")?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "https://localhost:7341", "https://localhost:7123", "https://cleanaspire.blazorserver.com" };
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins(allowedCorsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));


builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo<IEndpointRegistrar>())
    .As<IEndpointRegistrar>()
    .WithScopedLifetime());

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
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
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
app.MapEndpointDefinitions();
app.UseCors("wasm");
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
if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"files")))
    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"files"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"files")),
    RequestPath = new PathString("/files")
});
await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}



