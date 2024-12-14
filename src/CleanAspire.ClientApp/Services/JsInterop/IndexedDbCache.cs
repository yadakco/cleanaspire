// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.JSInterop;

namespace CleanAspire.ClientApp.Services.JsInterop;

public class IndexedDbCache
{
    public const string DATABASENAME = "CleanAspire.IndexedDB";
    private readonly IJSRuntime _jsRuntime;

    public IndexedDbCache(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // Save data to IndexedDB (generic version)
    public async Task SaveDataAsync<T>(string dbName, string key, T value)
    {
        await _jsRuntime.InvokeVoidAsync("indexedDbStorage.saveData", dbName, key, value);
    }

    // Get data from IndexedDB (generic version)
    public async Task<T> GetDataAsync<T>(string dbName, string key)
    {
        return await _jsRuntime.InvokeAsync<T>("indexedDbStorage.getData", dbName, key);
    }

    // Clear all data from IndexedDB store
    public async Task ClearDataAsync(string dbName)
    {
        await _jsRuntime.InvokeVoidAsync("indexedDbStorage.clearData", dbName);
    }
}
