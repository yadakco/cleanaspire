// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client;
using CleanAspire.ClientApp.Configurations;
using CleanAspire.ClientApp.Services;
using CleanAspire.ClientApp.Services.Identity;
using CleanAspire.ClientApp.Services.JsInterop;
using CleanAspire.ClientApp.Services.Proxies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Form;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Multipart;
using Microsoft.Kiota.Serialization.Text;
using CleanAspire.ClientApp;


var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();



// register the cookie handler
builder.Services.AddTransient<CookieHandler>();
builder.Services.AddTransient<WebpushrAuthHandler>();
builder.Services.AddScoped<UserProfileStore>();
builder.Services.AddScoped<OnlineStatusInterop>();
builder.Services.AddScoped<OfflineModeState>();
builder.Services.AddScoped<IndexedDbCache>();
builder.Services.AddScoped<ProductServiceProxy>();
builder.Services.AddScoped<OfflineSyncService>();

var clientAppSettings = builder.Configuration.GetSection(ClientAppSettings.KEY).Get<ClientAppSettings>();
builder.Services.AddSingleton(clientAppSettings!);

builder.Services.TryAddScopedMudBlazor(builder.Configuration);

var httpClientBuilder = builder.Services.AddHttpClient("apiservice", (sp, options) =>
{
    var settings = sp.GetRequiredService<ClientAppSettings>();
    options.BaseAddress = new Uri(settings.ServiceBaseUrl);

}).AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped<ApiClient>(sp =>
{
    ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultSerializer<TextSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultSerializer<FormSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultSerializer<MultipartSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();
    ApiClientBuilder.RegisterDefaultDeserializer<TextParseNodeFactory>();
    ApiClientBuilder.RegisterDefaultDeserializer<FormParseNodeFactory>();
    var settings = sp.GetRequiredService<ClientAppSettings>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("apiservice");
    var authProvider = new AnonymousAuthenticationProvider();
    var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
    var apiClient = new ApiClient(requestAdapter);
    if (!string.IsNullOrEmpty(settings.ServiceBaseUrl))
    {
        requestAdapter.BaseUrl = settings.ServiceBaseUrl;
    }
    return apiClient;

});
builder.Services.AddHttpClient("Webpushr", client =>
{
    client.BaseAddress = new Uri("https://api.webpushr.com");
}).AddHttpMessageHandler<WebpushrAuthHandler>();
builder.Services.AddScoped<WebpushrService>();

builder.Services.AddScoped<ApiClientService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("Local", options.ProviderOptions);
});
// register the custom state provider
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// register the account management interface
builder.Services.AddScoped(
    sp => (ISignInManagement)sp.GetRequiredService<AuthenticationStateProvider>());


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<CleanAspire.WebApp.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CleanAspire.ClientApp._Imports).Assembly);

app.Run();
