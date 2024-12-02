// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Products.DTOs;

namespace CleanAspire.Application.Features.Products.Queries;
public record GetProductByIdQuery(string Id) : IFusionCacheRequest<ProductDto?>
{
    public string CacheKey => $"product_{Id}";
    public IEnumerable<string>? Tags => new[] { "products" };
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProductByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
                            .Where(p => p.Id == request.Id)
                            .Select(p => new ProductDto
                            {
                                Id = p.Id,
                                SKU = p.SKU,
                                Name = p.Name,
                                Category = (ProductCategoryDto)p.Category,
                                Description = p.Description,
                                Price = p.Price,
                                Currency = p.Currency,
                                UOM = p.UOM
                            })
                            .SingleOrDefaultAsync(cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with Id '{request.Id}' was not found.");
        }
      
        return product;
    }
}
