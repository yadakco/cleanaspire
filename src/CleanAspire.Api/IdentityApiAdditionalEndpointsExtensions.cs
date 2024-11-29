// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CleanAspire.Domain.Identities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using CleanAspire.Application.Common.Interfaces;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
namespace CleanAspire.Api;

public static class IdentityApiAdditionalEndpointsExtensions
{
    // Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    public static IEndpointRouteBuilder MapIdentityApiAdditionalEndpoints<TUser>(this IEndpointRouteBuilder endpoints)
            where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<TUser>>();
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();
        string? confirmEmailEndpointName = null;
        var identityGroup = endpoints.MapGroup("/identity").RequireAuthorization().WithTags("Authentication", "Identity Management");
        identityGroup.MapPost("/logout", async (SignInManager<TUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok();
        })
        .WithSummary("Log out the current user.")
        .WithDescription("Logs out the currently authenticated user by signing them out of the system. This endpoint requires the user to be authorized before calling, and returns an HTTP 200 OK response upon successful logout.");

        identityGroup.MapGet("/profile", async Task<Results<Ok<ProfileResponse>, ValidationProblem, NotFound>>
            (ClaimsPrincipal claimsPrincipal, HttpContext context, IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
        })
        .WithSummary("Retrieve the user's profile")
        .WithDescription("Fetches the profile information of the authenticated user. " +
         "Returns 404 if the user is not found. Requires authorization.");
        identityGroup.MapPost("/profile", async Task<Results<Ok<ProfileResponse>, ValidationProblem, NotFound>>
            (ClaimsPrincipal claimsPrincipal, [FromBody] ProfileRequest request, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }

            if (!string.IsNullOrEmpty(request.Email) && !_emailAddressAttribute.IsValid(request.Email))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(request.Email)));
            }
            if (user is not ApplicationUser appUser)
                throw new InvalidCastException($"The provided user must be of type {nameof(ApplicationUser)}.");

            appUser.UserName = request.Username;
            appUser.Nickname = request.Nickname;
            appUser.TimeZoneId = request.TimeZoneId;
            appUser.LanguageCode = request.LanguageCode;
            appUser.SuperiorId = request.SuperiorId;
            appUser.TenantId = request.TenantId;
            appUser.AvatarUrl = request.AvatarUrl;
            await userManager.UpdateAsync(user).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(request.Email))
            {
                var email = await userManager.GetEmailAsync(user);

                if (email != request.Email)
                {
                    await SendConfirmationEmailAsync(user, userManager, context, request.Email, isChange: true);
                }
            }

            return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
        })
        .WithSummary("Update user profile information.")
        .WithDescription("Allows users to update their profile, including username, email, nickname, avatar, time zone, and language code.");



        var routeGroup = endpoints.MapGroup("/account").WithTags("Authentication", "Identity Management");
        routeGroup.MapPost("/signup", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] SignupRequest request, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            var user = new TUser();
            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(MapIdentityApiAdditionalEndpoints)} requires a user store with email support.");
            }
            if (user is not ApplicationUser appUser)
                throw new InvalidCastException($"The provided user must be of type {nameof(ApplicationUser)}.");
            appUser.Email = request.Email;
            appUser.UserName = request.Email;
            appUser.Nickname = request.Nickname;
            appUser.Provider = request.Provider;
            appUser.TenantId = request.TenantId;
            appUser.TimeZoneId = request.TimeZoneId;
            appUser.LanguageCode = request.LanguageCode;
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }
            await SendConfirmationEmailAsync(user, userManager, context, request.Email);
            return TypedResults.Ok();
        }).WithSummary("User Signup")
          .WithDescription("Allows a new user to sign up by providing required details such as email, password, and tenant-specific information. This endpoint creates a new user account and sends a confirmation email for verification.");

        routeGroup.MapGet("/confirmEmail", async Task<Results<ContentHttpResult, UnauthorizedHttpResult>>
            ([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            if (await userManager.FindByIdAsync(userId) is not { } user)
            {
                return TypedResults.Unauthorized();
            }
            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return TypedResults.Unauthorized();
            }
            IdentityResult result;
            if (string.IsNullOrEmpty(changedEmail))
            {
                result = await userManager.ConfirmEmailAsync(user, code);
            }
            else
            {
                result = await userManager.ChangeEmailAsync(user, changedEmail, code);
                if (result.Succeeded)
                {
                    result = await userManager.SetUserNameAsync(user, changedEmail);
                }
            }
            if (!result.Succeeded)
            {
                return TypedResults.Unauthorized();
            }
            return TypedResults.Text("Thank you for confirming your email.");
        }).WithSummary("Confirm Email or Update Email Address")
        .WithDescription("Processes email confirmation or email change requests for a user. It validates the confirmation code, verifies the user ID, and updates the email if a new one is provided. Returns a success message upon successful confirmation or email update.")
        .Add(endpointBuilder =>
        {
            var finalPattern = ((RouteEndpointBuilder)endpointBuilder).RoutePattern.RawText;
            confirmEmailEndpointName = $"{nameof(MapIdentityApiAdditionalEndpoints)}-{finalPattern}";
            endpointBuilder.Metadata.Add(new EndpointNameMetadata(confirmEmailEndpointName));
        });


        async Task SendConfirmationEmailAsync(TUser user, UserManager<TUser> userManager, HttpContext context, string email, bool isChange = false)
        {
            if (confirmEmailEndpointName is null)
            {
                throw new NotSupportedException("No email confirmation endpoint was registered!");
            }
            var code = isChange
                ? await userManager.GenerateChangeEmailTokenAsync(user, email)
                : await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var userId = await userManager.GetUserIdAsync(user);
            var routeValues = new RouteValueDictionary()
            {
                ["userId"] = userId,
                ["code"] = code,
            };

            if (isChange)
            {
                // This is validated by the /confirmEmail endpoint on change.
                routeValues.Add("changedEmail", email);
            }
            var confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
            ?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

            await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
        }
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
            AvatarUrl = appUser.AvatarUrl
        };
    }


    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        // We expect a single error code and description in the normal case.
        // This could be golfed with GroupBy and ToDictionary, but perf! :P
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }
}

public sealed class ProfileRequest
{
    [Description("User's preferred nickname.")]
    [MaxLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
    [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Nickname can only contain letters, numbers, and underscores.")]
    public string? Nickname { get; init; }

    [Description("Unique username for the user.")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    public required string Username { get; init; }
    [Required]
    [Description("User's email address. Must be in a valid email format.")]
    [MaxLength(80, ErrorMessage = "Email cannot exceed 80 characters.")]
    [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }
    [Description("User uploads an avatar image.")]
    public string? AvatarUrl { get; init; }
    [Description("User's time zone identifier, e.g., 'UTC', 'America/New_York'.")]
    [MaxLength(50, ErrorMessage = "TimeZoneId cannot exceed 50 characters.")]
    public string? TimeZoneId { get; set; }

    [Description("User's preferred language code, e.g., 'en-US'.")]
    [MaxLength(10, ErrorMessage = "LanguageCode cannot exceed 10 characters.")]
    [RegularExpression("^[a-z]{2,3}(-[A-Z]{2})?$", ErrorMessage = "Invalid language code format.")]
    public string? LanguageCode { get; set; }
    [Description("Tenant identifier for multi-tenant systems. Must be a GUID in version 7 format.")]
    [MaxLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
    public string? SuperiorId { get; init; }
    [Description("Tenant identifier for multi-tenant systems. Must be a GUID in version 7 format.")]
    [MaxLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
    public string? TenantId { get; set; }
}
public sealed class ProfileResponse
{
    public string? Nickname { get; init; }
    public string? Provider { get; init; }
    public string? TenantId { get; init; }
    public string? AvatarUrl { get; set; }
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required bool IsEmailConfirmed { get; init; }
    public string? TimeZoneId { get; init; }
    public string? LanguageCode { get; init; }
    public string? SuperiorId { get; init; }
}
public sealed class SignupRequest
{
    [Required]
    [Description("User's email address. Must be in a valid email format.")]
    [MaxLength(80, ErrorMessage = "Email cannot exceed 80 characters.")]
    [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }

    [Required]
    [Description("User's password. Must meet the security criteria.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [MaxLength(20, ErrorMessage = "Password cannot exceed 20 characters.")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,50}$", ErrorMessage = "Password must be at least 8 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.")]
    public required string Password { get; init; }

    [Description("User's preferred nickname.")]
    [MaxLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
    public string? Nickname { get; set; }

    [Description("Authentication provider, e.g., Local or Google.")]
    [MaxLength(20, ErrorMessage = "Provider cannot exceed 20 characters.")]
    public string? Provider { get; set; } = "Local";

    [Description("Tenant identifier for multi-tenant systems. Must be a GUID in version 7 format.")]
    [MaxLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
    public string? TenantId { get; set; }

    [Description("User's time zone identifier, e.g., 'UTC', 'America/New_York'.")]
    [MaxLength(50, ErrorMessage = "TimeZoneId cannot exceed 50 characters.")]
    public string? TimeZoneId { get; set; }

    [Description("User's preferred language code, e.g., 'en-US'.")]
    [MaxLength(10, ErrorMessage = "LanguageCode cannot exceed 10 characters.")]
    [RegularExpression("^[a-z]{2,3}(-[A-Z]{2})?$", ErrorMessage = "Invalid language code format.")]
    public string? LanguageCode { get; set; }
}


