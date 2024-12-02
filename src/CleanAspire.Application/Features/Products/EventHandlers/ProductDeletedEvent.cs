// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Products.EventHandlers;
public class ProductDeletedEvent : DomainEvent
{
    public ProductDeletedEvent(Product item)
    {
        Item = item;
    }
    public Product Item { get; }
}

/*
public class ProductDeletedEventHandler : INotificationHandler<ProductDeletedEvent>
{
    private readonly ILogger<ProductDeletedEventHandler> _logger;

    public ProductDeletedEventHandler(
        ILogger<ProductDeletedEventHandler> logger
    )
    {
        _logger = logger;
    }

    public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled domain event '{EventType}' with notification: {@Notification} in {ElapsedMilliseconds} ms", notification.GetType().Name, notification, _timer.ElapsedMilliseconds);
    }
}
*/
