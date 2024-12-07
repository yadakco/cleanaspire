using System;
using System.Threading;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using CleanAspire.PushNotifications.Services.Abstractions;

namespace CleanAspire.PushNotifications.Services.Cosmos;

internal class CosmosDbPushSubscriptionStore : IPushSubscriptionStore
{
    private readonly IPushSubscriptionCosmosDbClient _client;

    public CosmosDbPushSubscriptionStore(IPushSubscriptionCosmosDbClient client)
    {
        _client = client;
    }

    public Task StoreSubscriptionAsync(PushSubscription subscription)
    {
        return _client.AddAsync(subscription);
    }

    public Task DiscardSubscriptionAsync(string endpoint)
    {
        return _client.RemoveAsync(endpoint);
    }

    public Task ForEachSubscriptionAsync(Action<PushSubscription> action)
    {
        return ForEachSubscriptionAsync(action, CancellationToken.None);
    }

    public async Task ForEachSubscriptionAsync(Action<PushSubscription> action, CancellationToken cancellationToken)
    {
        await foreach (PushSubscription subscription in _client.GetAllAsync())
        {
            action(subscription);
        }
    }
}
