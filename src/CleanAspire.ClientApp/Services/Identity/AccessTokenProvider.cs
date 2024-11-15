// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.ClientApp.Configurations;
using Microsoft.Kiota.Abstractions.Authentication;

namespace CleanAspire.ClientApp.Services.Identity;

public class AccessTokenProvider : IAccessTokenProvider
{

    private readonly ClientAppSettings _clientAppSettings;

    public AccessTokenProvider (ClientAppSettings clientAppSettings)
    {
        _clientAppSettings = clientAppSettings;
    }

    public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator(new[] { _clientAppSettings.ServiceBaseUrl});

    public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        return string.Empty;
    }
}

