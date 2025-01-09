using System.Globalization;
using CleanAspire.Application.Features.Products.DTOs;
using CleanAspire.Application.Features.Products.EventHandlers;
using CleanAspire.Application.Pipeline;
using CsvHelper;

namespace CleanAspire.Application.Features.Products.Commands;
// A record that defines the ImportProductsCommand, which encapsulates the data needed to import products from a CSV file
public record ImportProductsCommand(Stream Stream // Stream containing CSV data
) : IFusionCacheRefreshRequest<Unit>,            // Implements interface for cache refresh requests
    IRequiresValidation                          // Implements interface for validation requirements
{
    // Optional tags for categorizing the command, useful for logging or debugging
    public IEnumerable<string>? Tags => new[] { "products" };
}

// Handler class responsible for processing the ImportProductsCommand
public class ImportProductsCommandHandler : IRequestHandler<ImportProductsCommand, Unit>
{
    private readonly IApplicationDbContext _context; // Database context for interacting with the data layer

    // Constructor to inject dependencies
    public ImportProductsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    // Asynchronously handles the ImportProductsCommand
    public async ValueTask<Unit> Handle(ImportProductsCommand request, CancellationToken cancellationToken)
    {
        // Resets the stream position to the beginning to ensure correct reading
        request.Stream.Position = 0;

        // Uses CsvHelper to read and parse the CSV data
        using (var reader = new StreamReader(request.Stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // Maps CSV records to ProductDto objects
            var records = csv.GetRecords<ProductDto>();

            // CsvHelper automatically handles parsing and mapping of CSV fields to object properties
            // based on their headers and property names, ensuring accuracy and reducing boilerplate code.

            // Iterates through each ProductDto and maps it to a Product entity
            foreach (var product in records.Select(x => new Product()
            {
                SKU = x.SKU,                        // Maps SKU from ProductDto
                Name = x.Name,                      // Maps Name from ProductDto
                Category = (ProductCategory)x.Category, // Maps Category from ProductDto
                Description = x.Description,        // Maps Description from ProductDto
                Price = x.Price,                    // Maps Price from ProductDto
                Currency = x.Currency,              // Maps Currency from ProductDto
                UOM = x.UOM                         // Maps Unit of Measure from ProductDto
            }))
            {
                // Adds a domain event for product creation
                product.AddDomainEvent(new ProductCreatedEvent(product));

                // Adds the new product to the database context
                _context.Products.Add(product);
            }

            // Saves changes asynchronously to the database
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Returns a Unit value to signal successful completion
        return Unit.Value;
    }
}
