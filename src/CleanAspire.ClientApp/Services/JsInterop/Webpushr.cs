// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.JSInterop;

namespace CleanAspire.ClientApp.Services.JsInterop;

public class Webpushr
{
    private readonly IJSRuntime _jsRuntime;

    public Webpushr(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetupWebpushrAsync(string key)
    {
        var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/webpushr.js");
        await module.InvokeVoidAsync("setupWebpushr", key);
    }
}
