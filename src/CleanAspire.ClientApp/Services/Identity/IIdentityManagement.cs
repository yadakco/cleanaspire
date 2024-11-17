// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Api.Client.Models;

namespace CleanAspire.ClientApp.Services.Identity;

public interface IIdentityManagement
{
    public Task<AccessTokenResponse> LoginAsync(LoginRequest request,bool remember=false, CancellationToken cancellationToken = default);
    public Task LogoutAsync(CancellationToken cancellationToken = default);
    public Task<Stream> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}

