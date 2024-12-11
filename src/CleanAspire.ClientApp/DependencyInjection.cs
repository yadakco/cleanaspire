
using MudBlazor.Services;
using MudBlazor;
using Blazored.LocalStorage;
using CleanAspire.ClientApp.Services.Interfaces;
using CleanAspire.ClientApp.Services.UserPreferences;
using CleanAspire.ClientApp.Services;
using System.Text.Json;
using CleanAspire.ClientApp.Services.IndexDb;
using System.Text.Json.Serialization.Metadata;
using CleanAspire.Api.Client.Models;
using System.Text.Json.Serialization;
using Tavenem.DataStorage;
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
        services.AddBlazoredLocalStorageAsSingleton();
        services.AddSingleton<IStorageService, LocalStorageService>();
        services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
        services.AddSingleton<LayoutService>();
        services.AddScoped<DialogServiceHelper>();
        #endregion
    }

    public static void AddIndexedDbService(this IServiceCollection services, IConfiguration config)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.TypeInfoResolverChain.Add(LocalItemContext.Default.WithAddedModifier(static typeInfo =>
        {
            if (typeInfo.Type == typeof(IIdItem))
            {
                typeInfo.PolymorphismOptions ??= new JsonPolymorphismOptions
                {
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
                };
                typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(LocalCredential), LocalCredential.ItemTypeName));
                typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(LocalProfileResponse), LocalProfileResponse.ItemTypeName));
            }
        }));

        services.AddIndexedDbService();
        services.AddIndexedDb(
            LocalItemContext.DATABASENAME,
            1,
            options);
    }


}

