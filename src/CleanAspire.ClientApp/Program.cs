using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CleanAspire.ClientApp;
using Microsoft.JSInterop;
using System.Globalization;
using CleanAspire.ClientApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var renderMode = Environment.GetEnvironmentVariable("BLAZOR_RENDER_MODE");

if (renderMode?.Equals("Standalone", StringComparison.OrdinalIgnoreCase) == true)
{
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");
}
// register the cookie handler
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddHttpClients(builder.Configuration);
builder.Services.AddAuthenticationAndLocalization(builder.Configuration);
var app = builder.Build();

await app.InitializeCultureAsync();

await app.RunAsync();

