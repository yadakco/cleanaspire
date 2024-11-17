// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CleanAspire.Domain.Identities;
namespace CleanAspire.Api;

public static class IdentityApiAdditionalEndpointsExtensions
{
    public static IEndpointRouteBuilder MapIdentityApiAdditionalEndpoints<TUser>(this IEndpointRouteBuilder endpoints)
            where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var routeGroup = endpoints.MapGroup("");
        routeGroup.MapPost("/logout", async (SignInManager<TUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok();
        }).RequireAuthorization()
        .WithTags("Authentication", "Identity Management")
        .WithSummary("Log out the current user.")
        .WithDescription("Logs out the currently authenticated user by signing them out of the system. This endpoint requires the user to be authorized before calling, and returns an HTTP 200 OK response upon successful logout.");
        routeGroup.MapGet("/profile", async Task<Results<Ok<ProfileResponse>, ValidationProblem, NotFound>>
            (ClaimsPrincipal claimsPrincipal, HttpContext context, IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
        }).RequireAuthorization()
        .WithTags("Authentication", "Identity Management")
        .WithSummary("Retrieve the profile information")
        .WithDescription("This endpoint fetches the profile information of the currently authenticated user based on their claims. " +
                 "If the user is not found in the system, it returns a 404 Not Found status. " +
                 "The endpoint requires authorization and utilizes Identity Management for user retrieval and profile generation.");
        return endpoints;



    }
    private static async Task<ProfileResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
        where TUser : class
    {
        if (user is not ApplicationUser appUser)
            throw new InvalidCastException($"The provided user must be of type {nameof(ApplicationUser)}.");
        return new()
        {
            UserId = await userManager.GetUserIdAsync(user) ?? throw new NotSupportedException("Users must have an ID."),
            Username = await userManager.GetUserNameAsync(user) ?? throw new NotSupportedException("Users must have a username."),
            Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
            TenantId = appUser.TenantId,
            Nickname = appUser.Nickname,
            LanguageCode = appUser.LanguageCode,
            Provider = appUser.Provider,
            SuperiorId = appUser.SuperiorId,
            TimeZoneId = appUser.TimeZoneId,
            Avatar = appUser.Avatar
        };
    }
}

public sealed class ProfileResponse
{
    public string? Nickname { get; init; }
    public string? Provider { get; init; }
    public string? TenantId { get; init; }
    public byte[]? Avatar { get; set; }
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required bool IsEmailConfirmed { get; init; }
    public string? TimeZoneId { get; init; }
    public string? LanguageCode { get; init; }
    public string? SuperiorId { get; init; }
}

