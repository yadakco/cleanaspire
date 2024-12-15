// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using CleanAspire.ClientApp.Services.JsInterop;
using Microsoft.Kiota.Abstractions;
using OneOf;


namespace CleanAspire.ClientApp.Services.Proxies;

public class ProductServiceProxy(ApiClient apiClient, IndexedDbCache indexedDbCache, OnlineStatusInterop onlineStatusInterop, OfflineModeState offlineModeState)
{
    private const string OfflineCreateCommandCacheKey = "OfflineCreateCommand:Product";
    public async Task<PaginatedResultOfProductDto> GetPaginatedProductsAsync(ProductsWithPaginationQuery paginationQuery)
    {
        var cacheKey = GeneratePaginationCacheKey(paginationQuery);
        var isOnline = await onlineStatusInterop.GetOnlineStatusAsync();

        if (isOnline)
        {
            var paginatedProducts = await apiClient.Products.Pagination.PostAsync(paginationQuery);

            if (paginatedProducts != null && offlineModeState.Enabled)
            {
                await indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, cacheKey, paginatedProducts, new[] { "products_pagination" });

                foreach (var productDto in paginatedProducts.Items)
                {
                    var productCacheKey = GenerateProductCacheKey(productDto.Id);
                    await indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, productDto, new[] { "product" });
                }
            }

            return paginatedProducts ?? new PaginatedResultOfProductDto();
        }
        else
        {
            var cachedPaginatedProducts = await indexedDbCache.GetDataAsync<PaginatedResultOfProductDto>(IndexedDbCache.DATABASENAME, cacheKey);
            if (cachedPaginatedProducts != null)
            {
                return cachedPaginatedProducts;
            }
        }

        return new PaginatedResultOfProductDto();
    }

    public async Task<OneOf<ProductDto, KeyNotFoundException>> GetProductByIdAsync(string productId)
    {
        var productCacheKey = GenerateProductCacheKey(productId);
        var isOnline = await onlineStatusInterop.GetOnlineStatusAsync();

        if (isOnline)
        {
            try
            {
                var productDetails = await apiClient.Products[productId].GetAsync();
                if (productDetails != null && offlineModeState.Enabled)
                {
                    await indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, productDetails, new[] { "product" });
                }
                return productDetails!;
            }
            catch
            {
                return new KeyNotFoundException($"Product with ID '{productId}' could not be fetched from the API.");
            }
        }
        else
        {
            var cachedProductDetails = await indexedDbCache.GetDataAsync<ProductDto>(IndexedDbCache.DATABASENAME, productCacheKey);
            if (cachedProductDetails != null)
            {
                return cachedProductDetails;
            }
            else
            {
                return new KeyNotFoundException($"Product with ID '{productId}' not found in offline cache.");
            }
        }
    }

    public async Task<OneOf<ProductDto, ApiException>> CreateProductAsync(CreateProductCommand command)
    {
        var isOnline = await onlineStatusInterop.GetOnlineStatusAsync();
        if (isOnline)
        {
            try
            {
                var response = await apiClient.Products.PostAsync(command);
                return response;
            }
            catch (ApiException ex)
            {
                return ex;
            }
        }
        else
        {
            var cachedCommands = await indexedDbCache.GetDataAsync<List<CreateProductCommand>>(IndexedDbCache.DATABASENAME, OfflineCreateCommandCacheKey)
                             ?? new List<CreateProductCommand>();
            cachedCommands.Add(command);
            await indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, OfflineCreateCommandCacheKey, cachedCommands, new[] { "product_commands" });
            var productDto = new ProductDto()
            {
                Id = Guid.CreateVersion7().ToString(),
                Category = command.Category,
                Currency = command.Currency,
                Description = command.Description,
                Name = command.Name,
                Price = command.Price,
                Sku = command.Sku,
                Uom = command.Uom
            };
            var productCacheKey = GenerateProductCacheKey(productDto.Id);
            await indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, productDto, new[] { "product" });
            var cachedPaginatedProducts = await indexedDbCache.GetDataByTagsAsync<PaginatedResultOfProductDto>(IndexedDbCache.DATABASENAME, new[] { "products_pagination" });
            if (cachedPaginatedProducts != null && cachedPaginatedProducts.Any())
            {
                foreach (var dic in cachedPaginatedProducts)
                {
                    var key = dic.Key;
                    var paginatedProducts = dic.Value;
                    paginatedProducts.Items.Insert(0,productDto);
                    paginatedProducts.TotalItems++;
                    await indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, key, paginatedProducts, new[] { "products_pagination" });
                }
            }
            return productDto;

        }
    }
    private string GeneratePaginationCacheKey(ProductsWithPaginationQuery query)
    {
        return $"{nameof(ProductsWithPaginationQuery)}:{query.PageNumber}_{query.PageSize}_{query.Keywords}_{query.OrderBy}_{query.SortDirection}";
    }
    private string GenerateProductCacheKey(string productId)
    {
        return $"{nameof(ProductDto)}:{productId}";
    }
}

