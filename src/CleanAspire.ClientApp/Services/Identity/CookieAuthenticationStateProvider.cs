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

public class CookieAuthenticationStateProvider(ApiClient apiClient, UserProfileStore profileStore) : AuthenticationStateProvider, IIdentityManagement
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
            var profileResponse = await apiClient.Identity.Profile.GetAsync();
            Console.WriteLine(profileResponse);
            profileStore.Set(profileResponse);
            if (profileResponse != null)
            {
                // in this example app, name and email are the same
                var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier,profileResponse.UserId),
                        new(ClaimTypes.Name,profileResponse.Username),
                        new(ClaimTypes.Email, profileResponse.Email),
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

    public async Task<AccessTokenResponse> LoginAsync(LoginRequest request, bool remember = false, CancellationToken cancellationToken = default)
    {
        try
        {
            // login with cookies
            await apiClient.Login.PostAsync(request, options =>
            {
                options.QueryParameters.UseCookies = remember;
                options.QueryParameters.UseSessionCookies = !remember;
            }, cancellationToken);
            // need to refresh auth state
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        }
        catch { }
        return new AccessTokenResponse();

    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await apiClient.Identity.Logout.PostAsync(cancellationToken: cancellationToken);
        // need to refresh auth state
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public Task<Stream> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

