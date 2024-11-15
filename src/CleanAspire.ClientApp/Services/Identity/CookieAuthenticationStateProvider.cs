// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace CleanAspire.ClientApp.Services.Identity;

public class CookieAuthenticationStateProvider(ApiClient apiClient) : AuthenticationStateProvider, IIdentityManagement
{
    private bool authenticated = false;
    private readonly ClaimsPrincipal unauthenticated = new(new ClaimsIdentity());
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        authenticated = false;

        // default to not authenticated
        var user = unauthenticated;

        try
        {
            // the user info endpoint is secured, so if the user isn't logged in this will fail
            var infoResponse = await apiClient.Manage.Info.GetAsync();
            Console.WriteLine(infoResponse);
            if (infoResponse != null)
            {
                // in this example app, name and email are the same
                var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, infoResponse.Email),
                        new(ClaimTypes.Email, infoResponse.Email),
                    };

                // set the principal
                var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                user = new ClaimsPrincipal(id);
                authenticated = true;
            }
        }
        catch { }

        // return the state
        return new AuthenticationState(user);
    }

    public async Task<AccessTokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // login with cookies
            await apiClient.Login.PostAsync(request, options => options.QueryParameters.UseCookies = true, cancellationToken);
                // need to refresh auth state
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
             
        }
        catch { }
        return new AccessTokenResponse();
      
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await apiClient.Logout.PostAsync(cancellationToken:cancellationToken);
        // need to refresh auth state
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public Task<Stream> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

