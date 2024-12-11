// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace CleanAspire.ClientApp.Services.IndexDb;

[JsonSerializable(typeof(IIdItem))]
[JsonSerializable(typeof(LocalProfileResponse))]
[JsonSerializable(typeof(LocalCredential))]
public partial class LocalItemContext : JsonSerializerContext
{
    public const string STORENAME = "LocalItemStore";
    public const string DATABASENAME = "CleanAspire.IndexedDB";
}


public class LocalProfileResponse : IdItem
{
    [JsonInclude]
    [JsonPropertyOrder(-1)]
    public override string IdItemTypeName
    {
        get => ItemTypeName;
        set { }
    }
    public const string ItemTypeName = ":LocalProfileResponse:";
    public string? Nickname { get; init; }
    public string? Provider { get; init; }
    public string? TenantId { get; init; }
    public string? AvatarUrl { get; set; }
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public string? TimeZoneId { get; init; }
    public string? LanguageCode { get; init; }
    public string? SuperiorId { get; init; }
}

public class LocalCredential : IdItem
{
    [JsonInclude]
    [JsonPropertyOrder(-1)]
    public override string IdItemTypeName
    {
        get => ItemTypeName;
        set { }
    }
    public const string ItemTypeName = ":LocalCredential:";
    public string? AccessToken { get; set; }
    public long? ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public string? TokenType { get; set; }
}

