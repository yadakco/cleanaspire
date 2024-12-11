// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using CleanAspire.Api.Client;
using CleanAspire.Api.Client.Models;
using CleanAspire.ClientApp.Services.IndexDb;
using CleanAspire.ClientApp.Services.JsInterop;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions;
using Tavenem.Blazor.IndexedDB;

namespace CleanAspire.ClientApp.Services.Identity;

public class CookieAuthenticationStateProvider(ApiClient apiClient, UserProfileStore profileStore, IServiceProvider serviceProvider) : AuthenticationStateProvider, IIdentityManagement
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
            var profileResponse = await apiClient.Account.Profile.GetAsync();
            profileStore.Set(profileResponse);
            if (profileResponse != null)
            {
                // in this example app, name and email are the same
                var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier,profileResponse.UserId),
                        new(ClaimTypes.Name,profileResponse.Username),
                        new(ClaimTypes.Email, profileResponse.Email),
                        new(ClaimTypes.GivenName, profileResponse.Nickname),
                        new(ClaimTypes.Locality, profileResponse.TimeZoneId),
                        new(ClaimTypes.Country, profileResponse.LanguageCode),
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

    public async Task<AccessTokenResponse> LoginAsync(LoginRequest request, bool enableOffline, bool remember = true, CancellationToken cancellationToken = default)
    {
        var indexedDb = serviceProvider.GetRequiredKeyedService<IndexedDb>("CleanAspire.IndexedDB");
        var onlineStatusService = serviceProvider.GetRequiredService<OnlineStatusService>();

        try
        {
            var isOnline = await onlineStatusService.GetOnlineStatusAsync();

            if (enableOffline)
            {
                if (isOnline)
                {
                    // Online login
                    var response = await apiClient.Login.PostAsync(request, options =>
                    {
                        options.QueryParameters.UseCookies = remember;
                        options.QueryParameters.UseSessionCookies = !remember;
                    }, cancellationToken);

                    // Store response in IndexedDB for offline access
                    await indexedDb[LocalItemContext.StoreName].StoreItemAsync(new LocalAccessTokenResponse()
                    {
                        AccessToken = request.Email,
                        ExpiresIn = int.MaxValue,
                        RefreshToken = request.Email,
                        TokenType = "Email"
                    });

                    // Return response
                    return response;
                }
                else
                {
                    // Offline login logic
                    var storedToken = await indexedDb[LocalItemContext.StoreName]
                        .Query<LocalAccessTokenResponse>()
                        .FirstOrDefaultAsync(x => x.AccessToken == request.Email);

                    if (storedToken != null)
                    {
                        // Return stored token as a successful login response
                        return new AccessTokenResponse
                        {
                            AccessToken = storedToken.AccessToken,
                            ExpiresIn = storedToken.ExpiresIn,
                            RefreshToken = storedToken.RefreshToken,
                            TokenType = storedToken.TokenType
                        };
                    }
                    else
                    {
                        // Throw exception if no offline data is available
                        throw new InvalidOperationException("No offline data available for the provided email.");
                    }
                }
            }
            else
            {
                // Online-only login
                var response = await apiClient.Login.PostAsync(request, options =>
                {
                    options.QueryParameters.UseCookies = remember;
                    options.QueryParameters.UseSessionCookies = !remember;
                }, cancellationToken);

                // Clear offline data for security
                await indexedDb[LocalItemContext.StoreName].ClearAsync();

                // Return response
                return response;
            }
        }
        catch (ApiException ex)
        {
            // Log and re-throw API exception
            throw;
        }
        catch (Exception ex)
        {
            // Log and re-throw general exception
            throw;
        }
        finally
        {
            // Refresh authentication state
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }


    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await apiClient.Account.Logout.PostAsync(cancellationToken: cancellationToken);
        // need to refresh auth state
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public Task<Stream> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

