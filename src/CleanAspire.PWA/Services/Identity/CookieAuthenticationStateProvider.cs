// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net.Http;
using System.Security.Claims;
using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace CleanAspire.PWA.Services.Identity;

public class CookieAuthenticationStateProvider(ApiClient apiClient) : AuthenticationStateProvider, IIdentityManagement
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await Task.Delay(100);
        //var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
        var user = new ClaimsPrincipal();
        return new AuthenticationState(user);
    }

    public async Task<AccessTokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // login with cookies
            var result = await apiClient.Login.PostAsync(request, options => options.QueryParameters.UseCookies = true, cancellationToken);
                // need to refresh auth state
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
             
        }
        catch { }
        return new AccessTokenResponse();
      
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

