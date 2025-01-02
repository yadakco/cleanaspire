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
    public async Task<OneOf<TResponse, HttpValidationProblemDetails, ProblemDetails>> ExecuteAsync<TResponse>(Func<Task<TResponse>> apiCall)
    {
        try
        {
            var result = await apiCall();
            return result;
        }
        catch (HttpValidationProblemDetails ex)
        {

            _logger.LogError(ex, ex.Message);
            return ex;
        }
        catch (ProblemDetails ex)
        {
            _logger.LogError(ex, ex.Message);
            return ex;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ProblemDetails
            {
                Title = ex.Message,
                Detail = ex.InnerException?.Message ?? ex.Message,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ProblemDetails
            {
                Title = ex.Message,
                Detail = ex.InnerException?.Message ?? ex.Message,
            };
        }
    }
}


