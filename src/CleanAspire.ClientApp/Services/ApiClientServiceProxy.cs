// This class provides a service proxy for interacting with an API, incorporating caching and error handling mechanisms.
// It uses IndexedDb for local caching and integrates with a logging system to handle API-related errors effectively.

// Purpose:
// 1. **Caching API Responses**:
//    - `QueryAsync` method retrieves data from the cache or fetches it via the provided factory function and stores it with optional tags and expiration.
//    - `ClearCache` method removes cached data associated with specific tags.

// 2. **Error-Handled API Calls**:
//    - `ExecuteAsync` method wraps API calls with robust error handling, returning either the response, validation issues, or generic problem details.
//    - Handles specific exceptions (e.g., `HttpValidationProblemDetails`, `ProblemDetails`, and `ApiException`) and logs errors for easier debugging.

// 3. **Seamless API Integration**:
//    - Simplifies calling APIs and managing cached data, improving performance and resilience of the application.

using CleanAspire.Api.Client.Models;
using CleanAspire.ClientApp.Services.JsInterop;
using Microsoft.Kiota.Abstractions;
using OneOf;

namespace CleanAspire.ClientApp.Services;

/// <summary>
/// Provides a service proxy for interacting with an API, incorporating caching and error handling mechanisms.
/// </summary>
/// <param name="logger">The logger instance for logging errors and information.</param>
/// <param name="cache">The IndexedDbCache instance for caching API responses locally.</param>
public class ApiClientServiceProxy(ILogger<ApiClientServiceProxy> logger, IndexedDbCache cache)
{
    /// <summary>
    /// Retrieves data from the cache or fetches it via the provided factory function and stores it with optional tags and expiration.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response data.</typeparam>
    /// <param name="cacheKey">The key used to identify the cached data.</param>
    /// <param name="factory">The factory function to fetch data if not present in the cache.</param>
    /// <param name="tags">Optional tags to associate with the cached data.</param>
    /// <param name="expiration">Optional expiration time for the cached data.</param>
    /// <returns>The cached or fetched data.</returns>
    public async Task<TResponse?> QueryAsync<TResponse>(string cacheKey, Func<Task<TResponse?>> factory, string[]? tags = null, TimeSpan? expiration = null)
    {
        cacheKey = $"{cacheKey}";
        return await cache.GetOrSetAsync(IndexedDbCache.DATABASENAME, cacheKey, factory, tags, expiration);
    }

    /// <summary>
    /// Removes cached data associated with specific tags.
    /// </summary>
    /// <param name="tags">The tags associated with the cached data to be removed.</param>
    public async Task ClearCache(string[] tags)
    {
        await cache.DeleteDataByTagsAsync(IndexedDbCache.DATABASENAME, tags);
    }

    /// <summary>
    /// Wraps API calls with robust error handling, returning either the response, validation issues, or generic problem details.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response data.</typeparam>
    /// <param name="apiCall">The API call function to be executed.</param>
    /// <returns>The result of the API call or error details.</returns>
    public async Task<OneOf<TResponse, HttpValidationProblemDetails, ProblemDetails>> ExecuteAsync<TResponse>(Func<Task<TResponse>> apiCall)
    {
        try
        {
            var result = await apiCall();
            return result;
        }
        catch (HttpValidationProblemDetails ex)
        {
            logger.LogError(ex, ex.Message);
            return ex;
        }
        catch (ProblemDetails ex)
        {
            logger.LogError(ex, ex.Message);
            return ex;
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, ex.Message);
            return new ProblemDetails
            {
                Title = ex.Message,
                Detail = ex.InnerException?.Message ?? ex.Message,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return new ProblemDetails
            {
                Title = ex.Message,
                Detail = ex.InnerException?.Message ?? ex.Message,
            };
        }
    }
}


