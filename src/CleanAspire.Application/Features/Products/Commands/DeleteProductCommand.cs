// A record that defines the DeleteProductCommand, which encapsulates the data needed to delete products by their IDs
using CleanAspire.Application.Features.Products.EventHandlers;
using CleanAspire.Application.Pipeline;

public record DeleteProductCommand(params IEnumerable<string> Ids) // Takes a list of product IDs as parameters
    : IFusionCacheRefreshRequest<Unit>,                           // Implements interface for cache refresh requests
      IRequiresValidation                                         // Implements interface for validation requirements
{
    // Optional tags for categorizing the command, useful for logging or debugging
    public IEnumerable<string>? Tags => new[] { "products" };
}

// Handler class responsible for processing the DeleteProductCommand
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _dbContext; // Database context for interacting with the data layer

    // Constructor to inject dependencies, including logger and database context
    public DeleteProductCommandHandler(ILogger<DeleteProductCommandHandler> logger, IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Asynchronously handles the DeleteProductCommand
    public async ValueTask<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        // Retrieves products from the database that match the provided IDs
        var products = _dbContext.Products.Where(p => request.Ids.Contains(p.Id));

        // Iterates through each product to add a deletion domain event and remove it from the database context
        foreach (var product in products)
        {
            product.AddDomainEvent(new ProductDeletedEvent(product)); // Adds domain event for product deletion
            _dbContext.Products.Remove(product);                      // Removes product from the database
        }

        // Saves changes asynchronously to the database
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Returns a Unit value to signal successful completion
        return Unit.Value;
    }
}
