// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client.Models;

namespace CleanAspire.PWA.Services.Identity;

public class AuthStoreEvent
{
    public AccessTokenResponse? AccessTokenResponse { get; private set; }


    public AuthStoreEvent(AccessTokenResponse? accessTokenResponse)
    {
        AccessTokenResponse = accessTokenResponse;
    }

    public override string ToString()
    {
        return $"token: {AccessTokenResponse?.AccessToken}";
    }
}

