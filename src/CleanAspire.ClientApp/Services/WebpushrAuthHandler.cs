// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using Microsoft.JSInterop;

namespace CleanAspire.ClientApp.Services;

public class WebpushrAuthHandler : DelegatingHandler
{
    private readonly ApiClient _apiClient;
    private readonly IJSRuntime _jsRuntime;

    public WebpushrAuthHandler(ApiClient apiClient, IJSRuntime jsRuntime)
    {
        _apiClient = apiClient;
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        WebpushrOptions? webpushrOptions;
        var cachedOptionsJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "WebpushrConfig");
        if (!string.IsNullOrEmpty(cachedOptionsJson))
        {
            webpushrOptions = JsonSerializer.Deserialize<WebpushrOptions>(cachedOptionsJson);
        }
        else
        {
            webpushrOptions = await _apiClient.Webpushr.Config.GetAsync();
            var optionsJson = JsonSerializer.Serialize(webpushrOptions);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "WebpushrConfig", optionsJson);
        }
        request.Headers.Clear();
        request.Headers.Add("webpushrKey", webpushrOptions?.ApiKey);
        request.Headers.Add("webpushrAuthToken", webpushrOptions?.Token);
        request.Headers.Add("Accept", "application/json");
        return await base.SendAsync(request, cancellationToken);
    }
}
