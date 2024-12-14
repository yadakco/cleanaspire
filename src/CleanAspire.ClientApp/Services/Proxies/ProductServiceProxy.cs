// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using CleanAspire.ClientApp.Services.JsInterop;


namespace CleanAspire.ClientApp.Services.Proxies;

public class ProductServiceProxy(ApiClient apiClient, IndexedDbCache indexedDb, OnlineStatusInterop onlineStatus, OfflineModeState offlineMode)
{
    public async Task<PaginatedResultOfProductDto> GetProductsAsync(ProductsWithPaginationQuery query)
    {
        var cacheKey = GenerateCacheKey(query);
        if (await onlineStatus.GetOnlineStatusAsync())
        {
            var apiResult = await apiClient.Products.Pagination.PostAsync(query);
            if (apiResult != null && offlineMode.Enabled)
            {
                await indexedDb.SaveDataAsync(IndexedDbCache.DATABASENAME, cacheKey, apiResult);
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
    private string GenerateCacheKey(ProductsWithPaginationQuery query)
    {
        return $"{nameof(ProductsWithPaginationQuery)}:{query.PageNumber}_{query.PageSize}_{query.Keywords}_{query.OrderBy}_{query.SortDirection}";
    }
}

