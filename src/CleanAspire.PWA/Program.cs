using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CleanAspire.PWA;
using CleanAspire.PWA.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions.Authentication;
using CleanAspire.Api.Client;
using CleanAspire.PWA.Services.Identity;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// register the cookie handler
builder.Services.AddTransient<CookieHandler>();

var clientAppSettings = builder.Configuration.GetSection(ClientAppSettings.KEY).Get<ClientAppSettings>();
builder.Services.AddSingleton(clientAppSettings);

builder.Services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
builder.Services.TryAddMudBlazor(builder.Configuration);
builder.Services.AddHttpClient("apiservice").AddHttpMessageHandler<CookieHandler>();
builder.Services.AddSingleton<ApiClient>(sp =>
{

    var settings = sp.GetRequiredService<IOptions<ClientAppSettings>>().Value;
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var authProvider = new AnonymousAuthenticationProvider();
    var httpClient = httpClientFactory.CreateClient("apiservice");
    var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
    var apiClient = new ApiClient(requestAdapter);
    if (!string.IsNullOrEmpty(settings.ServiceBaseUrl))
    {
        requestAdapter.BaseUrl = settings.ServiceBaseUrl;
    }
    return apiClient;

});
builder.Services.AddAuthorizationCore();
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
    sp => (IIdentityManagement)sp.GetRequiredService<AuthenticationStateProvider>());


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();



await app.RunAsync();
