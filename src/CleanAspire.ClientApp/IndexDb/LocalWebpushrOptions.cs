// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace CleanAspire.ClientApp.IndexDb;

public class LocalWebpushrOptions: IdItem
{
    [JsonInclude]
    [JsonPropertyOrder(-1)]
    public override string IdItemTypeName
    {
        get => ItemTypeName;
        set { }
    }
    public const string ItemTypeName = ":WebpushrOptions:";
    public string? ApiKey { get; init; }
    public string? PublicKey { get; init; }
    public string? Token { get; init; }
}
