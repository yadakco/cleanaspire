/*
This C# code defines a feature for updating product information in an application using the Clean Architecture principles. It includes:
UpdateProductCommand: A record that encapsulates the data needed to update a product.
Implements IFusionCacheRefreshRequest<Unit> and IRequiresValidation interfaces for cache refresh and validation support.
Includes properties such as Id, SKU, Name, Category, etc.
UpdateProductCommandHandler: A handler class that processes the UpdateProductCommand.
Uses IApplicationDbContext to interact with the database.
Updates the product details, raises a domain event (ProductUpdatedEvent), and persists changes to the database.
*/

using CleanAspire.Application.Features.Products.DTOs; // Import DTOs related to products.
using CleanAspire.Application.Features.Products.EventHandlers; // Import event handlers for product-related events.
using CleanAspire.Application.Pipeline; // Import application pipeline interfaces and behaviors.

namespace CleanAspire.Application.Features.Products.Commands; // Define the namespace for product commands.

// A record representing the command to update a product.
public record UpdateProductCommand(
    string Id, // Product ID.
    string SKU, // Stock Keeping Unit, a unique identifier for the product.
    string Name, // Product name.
    ProductCategoryDto? Category, // Optional category of the product.
    string? Description, // Optional product description.
    decimal Price, // Product price.
    string? Currency, // Optional currency code (e.g., USD, EUR).
    string? UOM // Optional Unit of Measurement (e.g., kg, pcs).
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
