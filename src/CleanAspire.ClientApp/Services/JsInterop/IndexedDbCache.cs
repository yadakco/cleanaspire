// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Microsoft.JSInterop;

namespace CleanAspire.ClientApp.Services.JsInterop;

public sealed class IndexedDbCache
{
    public const string DATABASENAME = "CleanAspire.IndexedDB";
    private readonly IJSRuntime _jsRuntime;

    public IndexedDbCache(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // Save data to IndexedDB with optional tags
    public async Task SaveDataAsync<T>(string dbName, string key, T value, string[]? tags = null, TimeSpan? expiration = null)
    {
        var expirationMs = expiration.HasValue ? (int)expiration.Value.TotalMilliseconds : (int?)null;
        await _jsRuntime.InvokeVoidAsync("indexedDbStorage.saveData", dbName, key, value, tags ?? Array.Empty<string>(), expirationMs);
    }
    // Get or set data in IndexedDB
    public async Task<T> GetOrSetAsync<T>(string dbName, string key, Func<Task<T>> factory, string[]? tags = null, TimeSpan? expiration = null)
    {
        var existingData = await GetDataAsync<T>(dbName, key);
        if (existingData != null)
        {
            return existingData;
        }

        var newData = await factory();
        await SaveDataAsync(dbName, key, newData, tags, expiration);
        return newData;
    }

    // Get data from IndexedDB by key
    public async Task<T> GetDataAsync<T>(string dbName, string key)
    {
        return await _jsRuntime.InvokeAsync<T>("indexedDbStorage.getData", dbName, key);
    }

    // Get all data by tags (supports array of tags)
    public async Task<Dictionary<string, T>> GetDataByTagsAsync<T>(string dbName, string[] tags)
    {
        // Call the JavaScript function and retrieve a list of { key, value }
        var results = await _jsRuntime.InvokeAsync<List<Dictionary<string, object>>>(
            "indexedDbStorage.getDataByTags", dbName, tags);

        // Convert the results to a dictionary
        return results.ToDictionary(
            result => result["key"].ToString(), // Extract the key as a string
            result =>
            {
                // Handle deserialization of 'value'
                var jsonElement = result["value"];
                return JsonSerializer.Deserialize<T>(jsonElement.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        );
    }
    // Delete specific data by key
    public async Task DeleteDataAsync(string dbName, string key)
    {
        await _jsRuntime.InvokeVoidAsync("indexedDbStorage.deleteData", dbName, key);
    }

    // Delete all data by tags (supports array of tags)
    public async Task DeleteDataByTagsAsync(string dbName, string[] tags)
    {
        await _jsRuntime.InvokeVoidAsync("indexedDbStorage.deleteDataByTags", dbName, tags);
    }

    // Clear all data from IndexedDB store
    public async Task ClearDataAsync(string dbName)
    {
        await _jsRuntime.InvokeVoidAsync("indexedDbStorage.clearData", dbName);
    }
}
