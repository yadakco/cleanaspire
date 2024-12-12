// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace CleanAspire.ClientApp.IndexDb;

[JsonSerializable(typeof(IIdItem))]
[JsonSerializable(typeof(LocalProfileResponse))]
[JsonSerializable(typeof(LocalCredential))]
public partial class LocalItemContext : JsonSerializerContext
{
    public const string STORENAME = "LocalItemStore";
    public const string DATABASENAME = "CleanAspire.IndexedDB";
}

