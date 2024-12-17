// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using CleanAspire.ClientApp.Services.JsInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using MudBlazor.Charts;
using OneOf;


namespace CleanAspire.ClientApp.Services.Proxies;

public class ProductServiceProxy
{
    private const string OFFLINECREATECOMMANDCACHEKEY = "OfflineCreateCommand:Product";
    private const string OFFLINEUPDATECOMMANDCACHEKEY = "OfflineUpdateCommand:Product";
    private readonly NavigationManager _navigationManager;
    private readonly WebpushrService _webpushrService;
    private readonly ApiClient _apiClient;
    private readonly IndexedDbCache _indexedDbCache;
    private readonly OnlineStatusInterop _onlineStatusInterop;
    private readonly OfflineModeState _offlineModeState;
    private readonly OfflineSyncService _offlineSyncService;
    private bool _previousOnlineStatus;
    public ProductServiceProxy(NavigationManager navigationManager, WebpushrService webpushrService, ApiClient apiClient, IndexedDbCache indexedDbCache, OnlineStatusInterop onlineStatusInterop, OfflineModeState offlineModeState, OfflineSyncService offlineSyncService)
    {
        _navigationManager = navigationManager;
        _webpushrService = webpushrService;
        _apiClient = apiClient;
        _indexedDbCache = indexedDbCache;
        _onlineStatusInterop = onlineStatusInterop;
        _offlineModeState = offlineModeState;
        _offlineSyncService = offlineSyncService;
        Initialize();
    }
    private void Initialize()
    {
        _onlineStatusInterop.OnlineStatusChanged -= OnOnlineStatusChanged;
        _onlineStatusInterop.OnlineStatusChanged += OnOnlineStatusChanged;
    }

    private async void OnOnlineStatusChanged(bool isOnline)
    {
        if (_previousOnlineStatus == isOnline)
            return;
        _previousOnlineStatus = isOnline;
        if (isOnline)
        {
            await SyncOfflineCachedDataAsync();
        }
    }
    public async Task<PaginatedResultOfProductDto> GetPaginatedProductsAsync(ProductsWithPaginationQuery paginationQuery)
    {
        var cacheKey = GeneratePaginationCacheKey(paginationQuery);
        var isOnline = await _onlineStatusInterop.GetOnlineStatusAsync();

        if (isOnline)
        {
            var paginatedProducts = await _apiClient.Products.Pagination.PostAsync(paginationQuery);

            if (paginatedProducts != null && _offlineModeState.Enabled)
            {
                await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, cacheKey, paginatedProducts, new[] { "products_pagination" });

                foreach (var productDto in paginatedProducts.Items)
                {
                    var productCacheKey = GenerateProductCacheKey(productDto.Id);
                    await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, productDto, new[] { "product" });
                }
            }

            return paginatedProducts ?? new PaginatedResultOfProductDto();
        }
        else
        {
            var cachedPaginatedProducts = await _indexedDbCache.GetDataAsync<PaginatedResultOfProductDto>(IndexedDbCache.DATABASENAME, cacheKey);
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
        var isOnline = await _onlineStatusInterop.GetOnlineStatusAsync();

        if (isOnline)
        {
            try
            {
                var productDetails = await _apiClient.Products[productId].GetAsync();
                if (productDetails != null && _offlineModeState.Enabled)
                {
                    await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, productDetails, new[] { "product" });
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
            var cachedProductDetails = await _indexedDbCache.GetDataAsync<ProductDto>(IndexedDbCache.DATABASENAME, productCacheKey);
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

    public async Task<OneOf<ProductDto, ApiClientValidationError, ApiClientError>> CreateProductAsync(CreateProductCommand command)
    {
        var isOnline = await _onlineStatusInterop.GetOnlineStatusAsync();
        if (isOnline)
        {
            try
            {
                var response = await _apiClient.Products.PostAsync(command);
                var baseUrl = _navigationManager.BaseUri.TrimEnd('/');
                var productUrl = $"{baseUrl}/products/edit/{response.Id}";
                await _webpushrService.SendNotificationAsync("New Product Launched!", $"Our new product, {response.Name}, is now available. Click to learn more!", $"{productUrl}");

                return response;
            }
            catch (HttpValidationProblemDetails ex)
            {
                return new ApiClientValidationError(ex.Detail, ex);
            }
            catch (ProblemDetails ex)
            {
                return new ApiClientError(ex.Detail, ex);
            }
            catch (ApiException ex)
            {
                return new ApiClientError(ex.Message, ex);
            }
            catch (Exception ex)
            {
                return new ApiClientError(ex.Message, ex);
            }
        }
        else
        {
            var cachedCommands = await _indexedDbCache.GetDataAsync<List<CreateProductCommand>>(IndexedDbCache.DATABASENAME, OFFLINECREATECOMMANDCACHEKEY)
                             ?? new List<CreateProductCommand>();
            cachedCommands.Add(command);
            await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, OFFLINECREATECOMMANDCACHEKEY, cachedCommands, new[] { "product_commands" });
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
            await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, productDto, new[] { "product" });
            var cachedPaginatedProducts = await _indexedDbCache.GetDataByTagsAsync<PaginatedResultOfProductDto>(IndexedDbCache.DATABASENAME, new[] { "products_pagination" });
            if (cachedPaginatedProducts != null && cachedPaginatedProducts.Any())
            {
                foreach (var dic in cachedPaginatedProducts)
                {
                    var key = dic.Key;
                    var paginatedProducts = dic.Value;
                    paginatedProducts.Items.Insert(0, productDto);
                    paginatedProducts.TotalItems++;
                    await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, key, paginatedProducts, new[] { "products_pagination" });
                }
            }
            return productDto;
        }
    }
    public async Task<OneOf<bool, ApiClientValidationError, ApiClientError>> UpdateProductAsync(UpdateProductCommand command)
    {
        var isOnline = await _onlineStatusInterop.GetOnlineStatusAsync();
        if (isOnline)
        {
            try
            {
                var response = await _apiClient.Products.PutAsync(command);
                return true;
            }
            catch (HttpValidationProblemDetails ex)
            {
                return new ApiClientValidationError(ex.Detail, ex);
            }
            catch (ProblemDetails ex)
            {
                return new ApiClientError(ex.Detail, ex);
            }
            catch (ApiException ex)
            {
                return new ApiClientError(ex.Message, ex);
            }
            catch (Exception ex)
            {
                return new ApiClientError(ex.Message, ex);
            }
        }
        else
        {
            var cachedCommands = await _indexedDbCache.GetDataAsync<List<UpdateProductCommand>>(IndexedDbCache.DATABASENAME, OFFLINEUPDATECOMMANDCACHEKEY)
                             ?? new List<UpdateProductCommand>();
            cachedCommands.Add(command);
            await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, OFFLINEUPDATECOMMANDCACHEKEY, cachedCommands, new[] { "product_commands" });
            var productDto = new ProductDto()
            {
                Id =command.Id,
                Category = command.Category,
                Currency = command.Currency,
                Description = command.Description,
                Name = command.Name,
                Price = command.Price,
                Sku = command.Sku,
                Uom = command.Uom
            };
            var productCacheKey = GenerateProductCacheKey(productDto.Id);
            await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, productCacheKey, productDto, new[] { "product" });
            var cachedPaginatedProducts = await _indexedDbCache.GetDataByTagsAsync<PaginatedResultOfProductDto>(IndexedDbCache.DATABASENAME, new[] { "products_pagination" });
            if (cachedPaginatedProducts != null && cachedPaginatedProducts.Any())
            {
                foreach (var dic in cachedPaginatedProducts)
                {
                    var key = dic.Key;
                    var paginatedProducts = dic.Value;
                    var item = paginatedProducts.Items.FirstOrDefault(x => x.Id == productDto.Id);
                    if (item != null)
                    {
                        item.Category = productDto.Category;
                        item.Currency = productDto.Currency;
                        item.Description = productDto.Description;
                        item.Name = productDto.Name;
                        item.Price = productDto.Price;
                        item.Sku = productDto.Sku;
                        item.Uom = productDto.Uom;
                    }
                    await _indexedDbCache.SaveDataAsync(IndexedDbCache.DATABASENAME, key, paginatedProducts, new[] { "products_pagination" });
                }
            }
            return true;
        }
    }
    public async Task<OneOf<bool,ApiException>> DeleteProductsAsync(List<string> productIds)
    {
        var isOnline = await _onlineStatusInterop.GetOnlineStatusAsync();
        if (isOnline)
        {
            try
            {
                await _apiClient.Products.DeleteAsync(new DeleteProductCommand() { Ids = productIds });
                await _indexedDbCache.DeleteDataByTagsAsync(IndexedDbCache.DATABASENAME, new[] { "products_pagination","product" });
                return true;
            }
            catch (ApiException ex)
            {
                return ex;
            }
        }
        return true;
    }
    public async Task SyncOfflineCachedDataAsync()
    {
        var cachedCreateProductCommands = await _indexedDbCache.GetDataAsync<List<CreateProductCommand>>(
        IndexedDbCache.DATABASENAME,
        OFFLINECREATECOMMANDCACHEKEY);
        if (cachedCreateProductCommands != null && cachedCreateProductCommands.Any())
        {
            var count = cachedCreateProductCommands.Count;
            var processedCount = 0;
            _offlineSyncService.SetSyncStatus(SyncStatus.Syncing, $"Starting sync: 0/{count} ...", count, processedCount);
            await Task.Delay(500);
            foreach (var command in cachedCreateProductCommands)
            {
                var result = await CreateProductAsync(command);
                result.Switch(
                    productDto =>
                    {
                        processedCount++;
                        _offlineSyncService.SetSyncStatus(SyncStatus.Syncing, $"Syncing {processedCount}/{count} Success.", count, processedCount);
                    },
                    invalid =>
                    {
                        processedCount++;
                        _offlineSyncService.SetSyncStatus(SyncStatus.Syncing, $"Syncing {processedCount}/{count} Failed ({invalid.Message}).", count, processedCount);
                    },
                    error =>
                    {
                        processedCount++;
                        _offlineSyncService.SetSyncStatus(SyncStatus.Syncing, $"Syncing {processedCount}/{count} Failed ({error.Message}).", count, processedCount);
                    });
                await Task.Delay(500);
            }
            _offlineSyncService.SetSyncStatus(SyncStatus.Completed, $"Sync completed: {processedCount}/{count} processed.", count, processedCount);
            await Task.Delay(1200);
            await _indexedDbCache.DeleteDataAsync(IndexedDbCache.DATABASENAME, OFFLINECREATECOMMANDCACHEKEY);
            _offlineSyncService.SetSyncStatus(SyncStatus.Idle, "", 0, 0);
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

