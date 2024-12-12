// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace CleanAspire.ClientApp.IndexDb;

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

