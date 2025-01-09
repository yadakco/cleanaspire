// This code defines a command and its handler for updating product details in the database.
// The UpdateProductCommand encapsulates the necessary data to update a product.
// The UpdateProductCommandHandler processes the command, validates existence, updates product details, triggers a domain event, and saves changes.


using CleanAspire.Application.Features.Products.DTOs; // Import DTOs related to products.
using CleanAspire.Application.Features.Products.EventHandlers; // Import event handlers for product-related events.
using CleanAspire.Application.Pipeline; // Import application pipeline interfaces and behaviors.

namespace CleanAspire.Application.Features.Products.Commands; // Define the namespace for product commands.

// A record representing the command to update a product.
public record UpdateProductCommand(
    string Id, // Product ID. corresponding to the Id field in ProductDto
    string SKU, // Stock Keeping Unit, a unique identifier for the product. corresponding to the SKU field in ProductDto
    string Name, // Product name. corresponding to the Name field in ProductDto
    ProductCategoryDto? Category, // Optional category of the product. corresponding to the ProductCategoryDto field in ProductDto
    string? Description, // Optional product description. corresponding to the Description field in ProductDto
    decimal Price, // Product price. corresponding to the Price field in ProductDto
    string? Currency, // Optional currency code (e.g., USD, EUR). corresponding to the Currency field in ProductDto
    string? UOM // Optional Unit of Measurement (e.g., kg, pcs). corresponding to the UOM field in ProductDto
) : IFusionCacheRefreshRequest<Unit>, // Interface for cache refresh requests.
    IRequiresValidation // Interface indicating that the command requires validation.
{
    // Tags associated with this command, used for caching or categorization.
    public IEnumerable<string>? Tags => new[] { "products" };
}

// Handler for processing the UpdateProductCommand.
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IApplicationDbContext _context; // Database context for accessing product data.

    // Constructor to inject the database context.
    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    // Asynchronously handles the update product command.
    public async ValueTask<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the product from the database using the provided ID.
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        if (product == null)
        {
            // Throw an exception if the product does not exist.
            throw new KeyNotFoundException($"Product with Id '{request.Id}' was not found.");
        }

        // Update the product properties with the values from the command.
        product.SKU = request.SKU;
        product.Name = request.Name;
        product.Category = request.Category.HasValue
            ? (ProductCategory)request.Category // Map to domain category if provided.
            : ProductCategory.Electronics; // Default to "Electronics" if no category is provided.
        product.Description = request.Description;
        product.Price = request.Price;
        product.Currency = request.Currency;
        product.UOM = request.UOM;

        // Add a domain event indicating the product has been updated.
        product.AddDomainEvent(new ProductUpdatedEvent(product));

        // Mark the product entity as modified.
        _context.Products.Update(product);

        // Save changes to the database.
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value; // Indicate successful completion.
    }
}
