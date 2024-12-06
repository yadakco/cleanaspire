// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client.Models;
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
    public async Task<OneOf<TResponse, ApiClientValidationError, ApiClientError>> ExecuteAsync<TResponse>(Func<Task<TResponse>> apiCall)
    {
        try
        {
            var result = await apiCall();
            return result;
        }
        catch (HttpValidationProblemDetails ex)
        {

            _logger.LogError(ex, ex.Message);
            return new ApiClientValidationError(ex.Detail, ex);
        }
        catch (ProblemDetails ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError(ex.Detail, ex);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError(ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError(ex.Message, ex);
        }
    }

    public async Task<OneOf<bool, ApiClientValidationError, ApiClientError>> ExecuteAsync(Func<Task> apiCall)
    {
        try
        {
            await apiCall();
            return true;
        }
        catch (HttpValidationProblemDetails ex)
        {

            _logger.LogError(ex, ex.Message);
            return new ApiClientValidationError(ex.Detail, ex);
        }
        catch (ProblemDetails ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError(ex.Detail, ex);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError(ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ApiClientError(ex.Message, ex);
        }
    }
}

public class ApiClientError
{
    public string? Message { get; }
    public Exception Exception { get; }

    public ApiClientError(string? message, Exception exception)
    {
        Message = message;
        Exception = exception;
    }
}
public class ApiClientValidationError
{
    public string? Message { get; }
    public HttpValidationProblemDetails Errors { get; }

    public ApiClientValidationError(string? message, HttpValidationProblemDetails details)
    {
        Message = message;
        Errors = details;
    }
}
