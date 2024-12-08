// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client;

namespace CleanAspire.ClientApp.Services;

public class WebpushrAuthHandler : DelegatingHandler
{
    private readonly ApiClient _apiClient;

    public WebpushrAuthHandler(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var webpushrOptions = await _apiClient.Webpushr.Config.GetAsync();
        request.Headers.Clear();
        request.Headers.Add("webpushrKey", webpushrOptions.ApiKey);
        request.Headers.Add("webpushrAuthToken", webpushrOptions.Token);
        request.Headers.Add("Accept", "application/json");
        return await base.SendAsync(request, cancellationToken);
    }
}
