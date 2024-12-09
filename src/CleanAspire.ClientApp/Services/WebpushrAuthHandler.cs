// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using CleanAspire.ClientApp.Services.Interfaces;
using Microsoft.JSInterop;

namespace CleanAspire.ClientApp.Services;

public class WebpushrAuthHandler : DelegatingHandler
{
    private readonly ApiClient _apiClient;
    private readonly IStorageService _localStorage;


    public WebpushrAuthHandler(ApiClient apiClient, IStorageService localStorage)
    {
        _apiClient = apiClient;
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        WebpushrOptions? webpushrOptions;
        webpushrOptions = await _localStorage.GetItemAsync<WebpushrOptions>("_webpushrConfig");
        if (webpushrOptions==null)
        {
            webpushrOptions = await _apiClient.Webpushr.Config.GetAsync();
            await _localStorage.SetItemAsync("_webpushrConfig", webpushrOptions);
        }
        request.Headers.Clear();
        request.Headers.Add("webpushrKey", webpushrOptions?.ApiKey);
        request.Headers.Add("webpushrAuthToken", webpushrOptions?.Token);
        request.Headers.Add("Accept", "application/json");
        return await base.SendAsync(request, cancellationToken);
    }
}
