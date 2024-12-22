using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CleanAspire.ClientApp;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using CleanAspire.ClientApp.Services.Identity;
using CleanAspire.ClientApp.Configurations;
using CleanAspire.Api.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Text;
using Microsoft.Kiota.Serialization.Form;
using Microsoft.Kiota.Serialization.Multipart;
using CleanAspire.ClientApp.Services;
using CleanAspire.ClientApp.Services.JsInterop;
using CleanAspire.ClientApp.Services.Proxies;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

// register the cookie handler
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddHttpClients(builder.Configuration);
builder.Services.AddAuthenticationAndLocalization(builder.Configuration);

var app = builder.Build();


await app.RunAsync();

