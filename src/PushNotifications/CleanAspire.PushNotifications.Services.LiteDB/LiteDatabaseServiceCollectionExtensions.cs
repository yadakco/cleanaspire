using Microsoft.Extensions.DependencyInjection;
using CleanAspire.PushNotifications.Services.Abstractions;

namespace CleanAspire.PushNotifications.Services.LiteDB;

public static class LiteDatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddLiteDatabasePushSubscriptionStore(this IServiceCollection services)
    {
        services.AddSingleton<IPushSubscriptionLiteDatabase, PushSubscriptionLiteDatabase>();

        services.AddSingleton<IPushSubscriptionStore, LiteDatabasePushSubscriptionStore>();
        services.AddSingleton<IPushSubscriptionStoreAccessorProvider, LiteDatabasePushSubscriptionStoreAccessorProvider>();

        return services;
    }
}
