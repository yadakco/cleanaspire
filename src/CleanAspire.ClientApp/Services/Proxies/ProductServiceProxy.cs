// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using CleanAspire.ClientApp.Services.JsInterop;
using Microsoft.Kiota.Abstractions;
using OneOf;


namespace CleanAspire.ClientApp.Services.Proxies;

public class ProductServiceProxy(ApiClient apiClient, IndexedDbCache indexedDb, OnlineStatusInterop onlineStatus, OfflineModeState offlineMode)
{
    public async Task<PaginatedResultOfProductDto> GetPaginationDataAsync(ProductsWithPaginationQuery query)
    {
        var cacheKey = GenerateCacheKey(query);
        var online = await onlineStatus.GetOnlineStatusAsync();
        if (online)
        {
            var apiResult = await apiClient.Products.Pagination.PostAsync(query);
            if (apiResult != null && offlineMode.Enabled)
            {
                await indexedDb.SaveDataAsync(IndexedDbCache.DATABASENAME, cacheKey, apiResult, new string[] { "product" });
                foreach (var dto in apiResult.Items)
                {
                    var productCacheKey = GenerateCacheKey(dto.Id);
                    await indexedDb.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, dto, new string[] { "product" });
                }
            }
            return apiResult ?? new PaginatedResultOfProductDto();
        }
        else
        {
            var cachResult = await indexedDb.GetDataAsync<PaginatedResultOfProductDto>(IndexedDbCache.DATABASENAME, cacheKey);
            if (cachResult != null)
            {
                return cachResult;
            }
        }
        return new PaginatedResultOfProductDto();
    }
    public async Task<OneOf<ProductDto, KeyNotFoundException>> GetProductByIdAsync(string id)
    {
        var productCacheKey = GenerateCacheKey(id);
        var online = await onlineStatus.GetOnlineStatusAsync();
        if (online)
        {
            try
            {
                var product = await apiClient.Products[id].GetAsync();
                if (product != null && offlineMode.Enabled)
                {
                    await indexedDb.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, product, new string[] { "product" });
                }
                return product!;
            }
            catch
            {
                return new KeyNotFoundException($"Product with ID '{id}' could not be fetched from the API.");
            }
        }
        else
        {
            var cacheProduct = await indexedDb.GetDataAsync<ProductDto>(IndexedDbCache.DATABASENAME, productCacheKey);
            if (cacheProduct != null)
            {
                return cacheProduct;
            }
            else
            {
                return new KeyNotFoundException($"Product with ID '{id}' not found in offline cache.");
            }
        }
    }
    private string GenerateCacheKey(ProductsWithPaginationQuery query)
    {
        return $"{nameof(ProductsWithPaginationQuery)}:{query.PageNumber}_{query.PageSize}_{query.Keywords}_{query.OrderBy}_{query.SortDirection}";
    }
    private string GenerateCacheKey(string id)
    {
        return $"{nameof(ProductDto)}:{id}";
    }
}

