using System.Threading.Tasks;
using System.Collections.Generic;
using Lib.Net.Http.WebPush;

namespace CleanAspire.PushNotifications.Services.Cosmos;

internal interface IPushSubscriptionCosmosDbClient
{
    Task EnsureCreatedAsync();

    Task AddAsync(PushSubscription subscription);

    Task RemoveAsync(string endpoint);

    IAsyncEnumerable<PushSubscription> GetAllAsync();
}
