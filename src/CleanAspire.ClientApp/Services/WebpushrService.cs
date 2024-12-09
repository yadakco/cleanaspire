// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net.Http.Headers;
using System.Text;

namespace CleanAspire.ClientApp.Services;

public class WebpushrService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebpushrService> _logger;

    public WebpushrService(IHttpClientFactory httpClientFactory, ILogger<WebpushrService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendNotificationAsync(string title, string message, string targetUrl, string? sid = null)
    {
        var client = _httpClientFactory.CreateClient("Webpushr");
        try
        {
            string payload = $@"{{
                ""title"": ""{title}"",
                ""message"": ""{message}"",
                ""target_url"": ""{targetUrl}"",
                ""icon"":""icon-192.png""
            }}";

            HttpContent httpContent = new StringContent(payload, Encoding.UTF8);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var url = sid == null ? "/v1/notification/send/all" : $"/v1/notification/send/sid/{sid}";
            var httpResponseMessage = await client.PostAsync(url, httpContent).ConfigureAwait(false);
            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                _logger.LogError($"Error sending notification: {responseContent}");
            }

            httpContent.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending notification.");
        }
        finally
        {
            client.Dispose();
        }
    }
}

