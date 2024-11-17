// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Identity;

namespace CleanAspire.Api;

public static class IdentityApiAdditionalEndpointsExtensions
{
    public static IEndpointRouteBuilder MapIdentityApiAdditionalEndpoints<TUser>(this IEndpointRouteBuilder endpoints)
            where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapPost("/logout", async (SignInManager<TUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok();
        }).RequireAuthorization()
        .WithTags("Authentication", "Identity Management")
        .WithSummary("Log out the current user.")
        .WithDescription("Logs out the currently authenticated user by signing them out of the system. This endpoint requires the user to be authorized before calling, and returns an HTTP 200 OK response upon successful logout.");

        return endpoints;
    }
}

