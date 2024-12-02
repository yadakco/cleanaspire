// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Kiota.Abstractions;
using OneOf;

namespace CleanAspire.ClientApp.Services;

public class ApiClientService
{
    private readonly ILogger<ApiClientService> _logger;

    public ApiClientService(ILogger<ApiClientService> logger)
    {
        _logger = logger;
    }
    public async Task<OneOf<TResponse, ApiClientError>> ExecuteAsync<TResponse>(Func<Task<TResponse>> apiCall)
    {
        try
        {
            var result = await apiCall();
            return result;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError("API error occurred.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError("An unexpected error occurred.", ex);
        }
    }

    public async Task<OneOf<bool, ApiClientError>> ExecuteAsync(Func<Task> apiCall)
    {
        try
        {
            await apiCall();
            return true;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError("API error occurred.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError("An unexpected error occurred.", ex);
        }
    }
}

public class ApiClientError
{
    public string Message { get; }
    public Exception Exception { get; }

    public ApiClientError(string message, Exception exception)
    {
        Message = message;
        Exception = exception;
    }
}
