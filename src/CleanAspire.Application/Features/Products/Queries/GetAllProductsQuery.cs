// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Products.DTOs;

namespace CleanAspire.Application.Features.Products.Queries;
public record GetAllProductsQuery() : IFusionCacheRequest<List<ProductDto>>
{
    public string CacheKey => "getallproducts";
    public IEnumerable<string>? Tags => new[] { "products" };
}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllProductsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products.OrderBy(x => x.Name)
            .Select(t => new ProductDto
            {
                Id = t.Id,
                SKU = t.SKU,
                Name = t.Name,
                Category =  (ProductCategoryDto)t.Category,
                Description = t.Description,
                Price = t.Price,
                Currency = t.Currency,
                UOM = t.UOM
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}
