using CleanAspire.Application.Features.Products.DTOs;

namespace CleanAspire.Application.Features.Products.Queries;
public record ProductsWithPaginationQuery(string Keywords, int PageNumber = 0, int PageSize = 15, string OrderBy = "Id", string SortDirection = "Descending") : IFusionCacheRequest<PaginatedResult<ProductDto>>
{
    public IEnumerable<string>? Tags => new[] { "products" };
    public string CacheKey => $"productswithpagination_{Keywords}_{PageNumber}_{PageSize}_{OrderBy}_{SortDirection}";
}

public class ProductsWithPaginationQueryHandler : IRequestHandler<ProductsWithPaginationQuery, PaginatedResult<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public ProductsWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<PaginatedResult<ProductDto>> Handle(ProductsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Products.OrderBy(request.OrderBy, request.SortDirection)
                    .ProjectToPaginatedDataAsync(
                        condition: x => x.SKU.Contains(request.Keywords) || x.Name.Contains(request.Keywords) || x.Description.Contains(request.Keywords),
                        pageNumber: request.PageNumber,
                        pageSize: request.PageSize,
                        mapperFunc: t => new ProductDto
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Description = t.Description,
                            Price = t.Price,
                            SKU = t.SKU,
                            UOM = t.UOM,
                            Currency = t.Currency,
                            Category = (ProductCategoryDto?)t.Category
                        },
                    cancellationToken: cancellationToken);

        return data;
    }
}
