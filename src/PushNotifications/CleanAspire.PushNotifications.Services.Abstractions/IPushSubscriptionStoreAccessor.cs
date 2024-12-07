using System;

namespace CleanAspire.PushNotifications.Services.Abstractions;

public interface IPushSubscriptionStoreAccessor : IDisposable
{
    IPushSubscriptionStore PushSubscriptionStore { get; }
}
