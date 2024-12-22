
using MudBlazor.Services;
using MudBlazor;
using Blazored.LocalStorage;
using CleanAspire.ClientApp.Services.Interfaces;
using CleanAspire.ClientApp.Services.UserPreferences;
using CleanAspire.ClientApp.Services;
using CleanAspire.ClientApp.Configurations;
using CleanAspire.ClientApp.Services.Identity;
using CleanAspire.ClientApp.Services.JsInterop;
using CleanAspire.ClientApp.Services.Proxies;
using CleanAspire.Api.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Form;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Multipart;
using Microsoft.Kiota.Serialization.Text;

namespace CleanAspire.ClientApp;

public static class DependencyInjection
{
    public static void TryAddMudBlazor(this IServiceCollection services, IConfiguration config)
    {
        #region register MudBlazor.Services
        services.AddMudServices(config =>
        {
            MudGlobal.InputDefaults.ShrinkLabel = true;
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;

            // we're currently planning on deprecating `PreventDuplicates`, at least to the end dev. however,
            // we may end up wanting to instead set it as internal because the docs project relies on it
            // to ensure that the Snackbar always allows duplicates. disabling the warning for now because
            // the project is set to treat warnings as errors.
#pragma warning disable 0618
            config.SnackbarConfiguration.PreventDuplicates = false;
#pragma warning restore 0618
        });
        services.AddMudPopoverService();
        services.AddMudBlazorSnackbar();
        services.AddMudBlazorDialog();
        services.AddMudLocalization();
        services.AddBlazoredLocalStorage();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        services.AddScoped<LayoutService>();
        services.AddScoped<DialogServiceHelper>();
        #endregion
    }


    public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Cookie and Authentication Handlers
        services.AddTransient<CookieHandler>();
        services.AddTransient<WebpushrAuthHandler>();

        // Singleton Services
        services.AddScoped<UserProfileStore>();
        services.AddScoped<OnlineStatusInterop>();
        services.AddScoped<OfflineModeState>();
        services.AddScoped<IndexedDbCache>();
        services.AddScoped<ProductServiceProxy>();
        services.AddScoped<OfflineSyncService>();
        services.AddScoped<WebpushrService>();

        // Configuration
        var clientAppSettings = configuration.GetSection(ClientAppSettings.KEY).Get<ClientAppSettings>();
        services.AddSingleton(clientAppSettings!);

        // MudBlazor Integration
        services.TryAddMudBlazor(configuration);
    }

    public static void AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpClient Registration
        services.AddHttpClient("apiservice", (sp, options) =>
        {
            var settings = sp.GetRequiredService<ClientAppSettings>();
            options.BaseAddress = new Uri(settings.ServiceBaseUrl);
        }).AddHttpMessageHandler<CookieHandler>();

        services.AddHttpClient("Webpushr", client =>
        {
            client.BaseAddress = new Uri("https://api.webpushr.com");
        }).AddHttpMessageHandler<WebpushrAuthHandler>();

        // ApiClient
        services.AddScoped<ApiClient>(sp =>
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

        // ApiClient Service
        services.AddScoped<ApiClientService>();
    }

    public static void AddAuthenticationAndLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        // Authentication and Authorization
        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        services.AddOidcAuthentication(options =>
        {
            configuration.Bind("Local", options.ProviderOptions);
        });
        services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
        services.AddScoped(sp => (ISignInManagement)sp.GetRequiredService<AuthenticationStateProvider>());

        // Localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");
    }
}

