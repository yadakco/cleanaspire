using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CleanAspire.ClientApp;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

// register the cookie handler
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddHttpClients(builder.Configuration);
builder.Services.AddAuthenticationAndLocalization(builder.Configuration);

var app = builder.Build();


await app.RunAsync();

