// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace CleanAspire.ClientApp.Services.JsInterop;

using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

public class OnlineStatusService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<OnlineStatusService>? _dotNetRef;

    public event Action<bool>? OnlineStatusChanged;

    public OnlineStatusService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    public async Task InitializeAsync()
    {
        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/onlinestatus.js");
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    public async Task<bool> GetOnlineStatusAsync()
    {
        if (_jsModule == null)
        {
            throw new InvalidOperationException("JavaScript module is not initialized. Call InitializeAsync first.");
        }
        return await _jsModule.InvokeAsync<bool>("getOnlineStatus");
    }
    public async Task RegisterOnlineStatusListenerAsync()
    {
        if (_jsModule == null)
        {
            throw new InvalidOperationException("JavaScript module is not initialized. Call InitializeAsync first.");
        }
        await _jsModule.InvokeVoidAsync("addOnlineStatusListener", _dotNetRef);
    }

    [JSInvokable]
    public void UpdateOnlineStatus(bool isOnline)
    {
        OnlineStatusChanged?.Invoke(isOnline);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dotNetRef != null)
        {
            _dotNetRef.Dispose();
        }
        if (_jsModule != null)
        {
            await _jsModule.DisposeAsync();
        }
    }
}
