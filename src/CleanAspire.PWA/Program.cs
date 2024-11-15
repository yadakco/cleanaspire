using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CleanAspire.PWA;
using CleanAspire.PWA.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions.Authentication;
using CleanAspire.Api.Client;
using CleanAspire.PWA.Services.Identity;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var clientAppSettings = builder.Configuration.GetSection(ClientAppSettings.KEY).Get<ClientAppSettings>();
builder.Services.AddSingleton(clientAppSettings);

builder.Services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
builder.Services.TryAddMudBlazor(builder.Configuration);
//builder.Services.AddSingleton<ApiClient>(sp =>
//{

//    var settings = sp.GetRequiredService<IOptions<ClientAppSettings>>().Value;
//    var accessTokenProvider = sp.GetRequiredService<IAccessTokenProvider>();
//    var authProvider = new BaseBearerTokenAuthenticationProvider(accessTokenProvider);
//    var requestAdapter = new HttpClientRequestAdapter(authProvider);
//    var apiClient = new ApiClient(requestAdapter);
//    if (!string.IsNullOrEmpty(settings.ServiceBaseUrl))
//    {
//        requestAdapter.BaseUrl = settings.ServiceBaseUrl;
//    }
//    return apiClient;

//});

builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("Local", options.ProviderOptions);
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

await builder.Build().RunAsync();
