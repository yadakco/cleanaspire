// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    public async Task SaveDataAsync<T>(string dbName, string key, T value, string[] tags = null)
    {
        await _jsRuntime.InvokeVoidAsync("indexedDbStorage.saveData", dbName, key, value, tags ?? Array.Empty<string>());
    }

    // Get data from IndexedDB by key
    public async Task<T> GetDataAsync<T>(string dbName, string key)
    {
        return await _jsRuntime.InvokeAsync<T>("indexedDbStorage.getData", dbName, key);
    }

    // Get all data by tags (supports array of tags)
    public async Task<List<T>> GetDataByTagsAsync<T>(string dbName, string[] tags)
    {
        var results = await _jsRuntime.InvokeAsync<List<dynamic>>("indexedDbStorage.getDataByTags", dbName, tags);
        return results.Select(result => (T)result.value).ToList();
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
