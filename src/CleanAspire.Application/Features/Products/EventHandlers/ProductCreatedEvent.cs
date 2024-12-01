namespace CleanAspire.Application.Features.Products.EventHandlers;
public class ProductCreatedEvent : DomainEvent
{
    public ProductCreatedEvent(Product item)
    {
        Item = item;
    }
    public Product Item { get; }
}
/*
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(
        ILogger<ProductCreatedEventHandler> logger
    )
    {
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled domain event '{EventType}' with notification: {@Notification} in {ElapsedMilliseconds} ms", notification.GetType().Name, notification, _timer.ElapsedMilliseconds);
    }
}
*/
