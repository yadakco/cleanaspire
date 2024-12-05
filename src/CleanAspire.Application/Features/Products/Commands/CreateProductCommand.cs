using CleanAspire.Application.Features.Products.DTOs;
using CleanAspire.Application.Features.Products.EventHandlers;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Products.Commands;
public record CreateProductCommand(
    string SKU,
    string Name,
    ProductCategoryDto? Category,
    string? Description,
    decimal Price,
    string? Currency,
    string? UOM
) : IFusionCacheRefreshRequest<ProductDto>, IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "products" };
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            SKU = request.SKU,
            Name = request.Name,
            Category = (ProductCategory)request.Category,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency,
            UOM = request.UOM
        };
        product.AddDomainEvent(new ProductCreatedEvent(product));
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProductDto() { Id = product.Id, Name = product.Name, SKU = product.SKU };
    }
}
