using System;
using CleanAspire.PushNotifications.Services.Abstractions;

namespace CleanAspire.PushNotifications.Services.LiteDB;

internal class LiteDatabasePushSubscriptionStoreAccessorProvider : IPushSubscriptionStoreAccessorProvider
{
    private readonly IPushSubscriptionStore _pushSubscriptionStore;

    public LiteDatabasePushSubscriptionStoreAccessorProvider(IPushSubscriptionStore pushSubscriptionStore)
    {
        _pushSubscriptionStore = pushSubscriptionStore;
    }

    public IPushSubscriptionStoreAccessor GetPushSubscriptionStoreAccessor()
    {
        return new LiteDatabasePushSubscriptionStoreAccessor(_pushSubscriptionStore);
    }
}
