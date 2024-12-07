// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.PushNotifications;
using CleanAspire.PushNotifications.Services.Abstractions;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

public class PushNotificationsEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        // Create a group for all push notifications related endpoints.
        // This allows applying shared attributes like tags or policies at once.
        var group = routes
            .MapGroup("/pushNotifications")
            .WithTags("Push Notifications")
            .AllowAnonymous();

        // GET /push-notifications-api/public-key
        group.MapGet("/publicKey", (IPushNotificationService notificationService) =>
        {
            // Returns the public key as plain text.
            return TypedResults.Ok(notificationService.PublicKey);
        }).Produces<string>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get public key")
        .WithDescription("Returns the public key used for push notifications."); ;

        // POST /push-notifications-api/subscriptions
        group.MapPost("/subscriptions", async ([FromBody] PushSubscription subscription, IPushSubscriptionStore subscriptionStore) =>
        {
            // Stores the incoming push subscription.
            await subscriptionStore.StoreSubscriptionAsync(subscription);
            return TypedResults.Ok();
        }).Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Store subscription")
        .WithDescription("Saves a new push subscription for receiving notifications."); ;

        // DELETE /push-notifications-api/subscriptions?endpoint={endpoint}
        group.MapDelete("/subscriptions", async ([FromQuery] string endpoint, IPushSubscriptionStore subscriptionStore) =>
        {
            // Discards an existing subscription by its endpoint.
            await subscriptionStore.DiscardSubscriptionAsync(endpoint);
            return TypedResults.Ok();
        }).ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Discard subscription")
        .WithDescription("Removes an existing push subscription identified by its endpoint.");
        ;

        // POST /push-notifications-api/notifications
        group.MapPost("/notifications", ([FromBody] PushMessageRequest message, IPushNotificationsQueue pushNotificationsQueue) =>
        {
            // Enqueues a new push notification message.
            pushNotificationsQueue.Enqueue(new PushMessage(message.Notification)
            {
                Topic = message.Topic,
                Urgency = message.Urgency
            });

            return TypedResults.Ok();
        }).Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Send notification")
        .WithDescription("Enqueues a push notification to be sent to subscribers."); ;
    }

}
