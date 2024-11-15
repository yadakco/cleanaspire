// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client.Models;

namespace CleanAspire.PWA.Services.Identity;

public class AuthStore
{
    public event EventHandler<AuthStoreEvent>? OnChange;
    public AccessTokenResponse? AccessTokenResponse { get; private set; }

    public void Set(AccessTokenResponse accessTokenResponse)
    {
        AccessTokenResponse = accessTokenResponse;
        OnChange?.Invoke(this, new AuthStoreEvent(AccessTokenResponse));
    }

    public void Clear()
    {
        AccessTokenResponse = null;
        OnChange?.Invoke(this, new AuthStoreEvent(AccessTokenResponse));
    }
}

