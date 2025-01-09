/*This code defines a CreateProductCommand and its handler to create a new product within a CQRS architecture. 
 * The CreateProductCommand encapsulates necessary data such as SKU, Name, Category, Description, Price, Currency, and UOM. 
 * It implements IFusionCacheRefreshRequest<ProductDto> for cache updates and IRequiresValidation for data validation. The handler,
 * CreateProductCommandHandler, processes the command by creating a Product entity, 
 * adding a ProductCreatedEvent domain event, saving the product to the database via IApplicationDbContext, 
 * and returning a ProductDto with the product's basic information. 
 * This design ensures clear separation of concerns and maintainability.
 */

// Using directives for necessary namespaces, bringing in external types used in the code
using CleanAspire.Application.Features.Products.DTOs; // Contains data transfer objects (DTOs) for the Product feature
using CleanAspire.Application.Features.Products.EventHandlers; // Contains event handlers related to Product events
using CleanAspire.Application.Pipeline; // Contains pipeline behaviors and related interfaces

// Namespace for organizing related classes and features
namespace CleanAspire.Application.Features.Products.Commands;

// A record that defines the CreateProductCommand, which encapsulates the data needed to create a new product
public record CreateProductCommand(
    string SKU,                          // The stock-keeping unit, a unique identifier for the product
    string Name,                         // The name of the product
    ProductCategoryDto? Category,        // The category of the product, nullable, referencing ProductCategoryDto definition
    string? Description,                 // A description of the product, nullable, corresponding to the Description field in ProductDto
    decimal Price,                       // The price of the product, matching the Price field in ProductDto
    string? Currency,                    // The currency of the price, nullable, referencing the Currency field in ProductDto
    string? UOM                          // The unit of measure for the product, nullable, consistent with the UOM field in ProductDto
) : IFusionCacheRefreshRequest<ProductDto>, // Implements interface for cache refresh requests
    IRequiresValidation                 // Implements interface for validation requirements
{
    // Optional tags for categorizing the command, useful for logging or debugging
    public IEnumerable<string>? Tags => new[] { "products" };
}

// Handler class responsible for processing the CreateProductCommand and returning the result
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context; // Database context for interacting with the data layer

    // Constructor to inject dependencies
    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    // Asynchronously handles the CreateProductCommand
    public async ValueTask<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Creates a new Product entity using the data from the command
        var product = new Product
        {
            SKU = request.SKU,                        // Assigns SKU from command
            Name = request.Name,                      // Assigns Name from command
            Category = (ProductCategory)request.Category, // Maps Category DTO to domain entity
            Description = request.Description,        // Assigns Description from command
            Price = request.Price,                    // Assigns Price from command
            Currency = request.Currency,              // Assigns Currency from command
            UOM = request.UOM                         // Assigns Unit of Measure from command
        };

        // Adds a domain event to signal that a new product has been created
        product.AddDomainEvent(new ProductCreatedEvent(product));

        // Adds the new product to the database context
        _context.Products.Add(product);

        // Saves changes asynchronously to the database
        await _context.SaveChangesAsync(cancellationToken);

        // Returns a ProductDto containing essential information about the created product
        return new ProductDto() { Id = product.Id, Name = product.Name, SKU = product.SKU };
    }
}
