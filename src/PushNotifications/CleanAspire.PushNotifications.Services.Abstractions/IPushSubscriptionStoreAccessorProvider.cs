namespace CleanAspire.PushNotifications.Services.Abstractions;

public interface IPushSubscriptionStoreAccessorProvider
{
    IPushSubscriptionStoreAccessor GetPushSubscriptionStoreAccessor();
}
