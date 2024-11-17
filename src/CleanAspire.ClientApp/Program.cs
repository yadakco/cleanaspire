using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CleanAspire.ClientApp;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
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

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// register the cookie handler
builder.Services.AddTransient<CookieHandler>();
builder.Services.AddSingleton<UserProfileStore>();

var clientAppSettings = builder.Configuration.GetSection(ClientAppSettings.KEY).Get<ClientAppSettings>();
builder.Services.AddSingleton(clientAppSettings);

builder.Services.TryAddMudBlazor(builder.Configuration);
builder.Services.AddHttpClient("apiservice", (sp,options) => {
    var settings = sp.GetRequiredService<IOptions<ClientAppSettings>>().Value;
    options.BaseAddress = new Uri(settings.ServiceBaseUrl);

}).AddHttpMessageHandler<CookieHandler>();
builder.Services.AddSingleton<ApiClient>(sp =>
{
    ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultSerializer<TextSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultSerializer<FormSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultSerializer<MultipartSerializationWriterFactory>();
    ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();
    ApiClientBuilder.RegisterDefaultDeserializer<TextParseNodeFactory>();
    ApiClientBuilder.RegisterDefaultDeserializer<FormParseNodeFactory>();
    var settings = sp.GetRequiredService<IOptions<ClientAppSettings>>().Value;
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
    sp => (IIdentityManagement)sp.GetRequiredService<AuthenticationStateProvider>());


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();



await app.RunAsync();

