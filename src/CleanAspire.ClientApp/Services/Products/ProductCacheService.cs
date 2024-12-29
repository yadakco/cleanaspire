// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using CleanAspire.Api.Client.Models;
using CleanAspire.ClientApp.Services.JsInterop;


namespace CleanAspire.ClientApp.Services.Products;

public class ProductCacheService
{
    private readonly IndexedDbCache _cache;
    public const string DATABASENAME = IndexedDbCache.DATABASENAME;
    private const string OFFLINE_CREATE_COMMAND_CACHE_KEY = "OfflineCreateCommand:product";
    private const string OFFLINE_UPDATE_COMMAND_CACHE_KEY = "OfflineUpdateCommand:product";
    private const string OFFLINE_DELETE_COMMAND_CACHE_KEY = "OfflineDeleteCommand:product";
    private const string OFFLINE_PAGINATION_CACHE_TAG = "OfflinePagination:";
    private const string PAGINATION_TAG = "products_pagination";
    private const string COMMANDS_TAG = "product_commands";
    private const string OBJECT_TAG = "product";

    public ProductCacheService(IndexedDbCache cache)
    {
        _cache = cache;
    }

    public async Task SaveOrUpdateProductAsync(ProductDto product)
    {
        var productCacheKey = GenerateProductCacheKey(product.Id);
        await _cache.SaveDataAsync(DATABASENAME, productCacheKey, product, new[] { OBJECT_TAG });
    }

    public async Task<ProductDto?> GetProductAsync(string productId)
    {
        var productCacheKey = GenerateProductCacheKey(productId);
        return await _cache.GetDataAsync<ProductDto>(DATABASENAME, productCacheKey);
    }

    public async Task SaveOrUpdatePaginatedProductsAsync(ProductsWithPaginationQuery query, PaginatedResultOfProductDto data)
    {
        var cacheKey = GeneratePaginationCacheKey(query);
        await _cache.SaveDataAsync(DATABASENAME, cacheKey, data, new[] { PAGINATION_TAG });
    }
    public async Task SaveOrUpdatePaginatedProductsAsync(string cacheKey, PaginatedResultOfProductDto data)
    {
        await _cache.SaveDataAsync(DATABASENAME, cacheKey, data, new[] { PAGINATION_TAG });
    }
    public async Task<PaginatedResultOfProductDto?> GetPaginatedProductsAsync(ProductsWithPaginationQuery query)
    {
        var cacheKey = GeneratePaginationCacheKey(query);
        return await _cache.GetDataAsync<PaginatedResultOfProductDto>(DATABASENAME, cacheKey);
    }
    public async Task<Dictionary<string, PaginatedResultOfProductDto>> GetAllCachedPaginatedResultsAsync()
    {
        var cachedData = await _cache.GetDataByTagsAsync<PaginatedResultOfProductDto>(
                DATABASENAME, new[] { PAGINATION_TAG }
            );
        return cachedData ?? new Dictionary<string, PaginatedResultOfProductDto>();
    }
    public async Task DeleteProductAsync(string productId)
    {
        var productCacheKey = GenerateProductCacheKey(productId);
        await _cache.DeleteDataAsync(DATABASENAME, productCacheKey);
    }

    public async Task UpdateDeletedProductsAsync(List<string> productIds)
    {
        foreach (var pid in productIds)
        {
            await DeleteProductAsync(pid);
        }
        var cachedPaginatedProducts = await _cache
            .GetDataByTagsAsync<PaginatedResultOfProductDto>(DATABASENAME, new[] { PAGINATION_TAG });

        if (cachedPaginatedProducts != null && cachedPaginatedProducts.Any())
        {
            foreach (var kvp in cachedPaginatedProducts)
            {
                var key = kvp.Key;
                var paginatedProducts = kvp.Value;
                paginatedProducts.Items = paginatedProducts.Items
                    .Where(p => !productIds.Contains(p.Id))
                    .ToList();

                paginatedProducts.TotalItems = paginatedProducts.Items.Count;
                await _cache.SaveDataAsync(
                    DATABASENAME,
                    key,
                    paginatedProducts,
                    new[] { PAGINATION_TAG }
                );
            }
        }
    }

    public async Task StoreOfflineCreateCommandAsync(CreateProductCommand command)
    {
        var cached = await _cache.GetDataAsync<List<CreateProductCommand>>(
            DATABASENAME, OFFLINE_CREATE_COMMAND_CACHE_KEY
        ) ?? new List<CreateProductCommand>();

        cached.Add(command);
        await _cache.SaveDataAsync(DATABASENAME, OFFLINE_CREATE_COMMAND_CACHE_KEY, cached, new[] {  COMMANDS_TAG });
    }

    public async Task StoreOfflineUpdateCommandAsync(UpdateProductCommand command)
    {
        var cached = await _cache.GetDataAsync<List<UpdateProductCommand>>(
            DATABASENAME, OFFLINE_UPDATE_COMMAND_CACHE_KEY
        ) ?? new List<UpdateProductCommand>();

        cached.Add(command);
        await _cache.SaveDataAsync(DATABASENAME, OFFLINE_UPDATE_COMMAND_CACHE_KEY, cached, new[] { COMMANDS_TAG });
    }

    public async Task StoreOfflineDeleteCommandAsync(DeleteProductCommand command)
    {
        var cached = await _cache.GetDataAsync<List<DeleteProductCommand>>(
            DATABASENAME, OFFLINE_DELETE_COMMAND_CACHE_KEY
        ) ?? new List<DeleteProductCommand>();

        cached.Add(command);
        await _cache.SaveDataAsync(DATABASENAME, OFFLINE_DELETE_COMMAND_CACHE_KEY, cached, new[] { COMMANDS_TAG });
    }

    public async Task<Tuple<int, List<CreateProductCommand>, List<UpdateProductCommand>, List<DeleteProductCommand>>> GetAllPendingCommandsAsync()
    {
        var createCommands = await _cache.GetDataAsync<List<CreateProductCommand>>(
            IndexedDbCache.DATABASENAME, OFFLINE_CREATE_COMMAND_CACHE_KEY
        ) ?? new List<CreateProductCommand>();

        var updateCommands = await _cache.GetDataAsync<List<UpdateProductCommand>>(
            IndexedDbCache.DATABASENAME, OFFLINE_UPDATE_COMMAND_CACHE_KEY
        ) ?? new List<UpdateProductCommand>();

        var deleteCommands = await _cache.GetDataAsync<List<DeleteProductCommand>>(
            IndexedDbCache.DATABASENAME, OFFLINE_DELETE_COMMAND_CACHE_KEY
        ) ?? new List<DeleteProductCommand>();

        return new Tuple<int, List<CreateProductCommand>, List<UpdateProductCommand>, List<DeleteProductCommand>>(
            createCommands.Count + updateCommands.Count + deleteCommands.Count,
            createCommands,
            updateCommands,
            deleteCommands
        );
    }

    public async Task ClearCommands()
    {
        await _cache.DeleteDataAsync(DATABASENAME, OFFLINE_CREATE_COMMAND_CACHE_KEY);
        await _cache.DeleteDataAsync(DATABASENAME, OFFLINE_UPDATE_COMMAND_CACHE_KEY);
        await _cache.DeleteDataAsync(DATABASENAME, OFFLINE_DELETE_COMMAND_CACHE_KEY);
    }
    private string GenerateProductCacheKey(string productId)
    {
        return $"{nameof(ProductDto)}:{productId}";
    }
    private string GeneratePaginationCacheKey(ProductsWithPaginationQuery query)
    {
        return $"{nameof(ProductsWithPaginationQuery)}:{query.PageNumber}_{query.PageSize}_{query.Keywords}_{query.OrderBy}_{query.SortDirection}";
    }
}
