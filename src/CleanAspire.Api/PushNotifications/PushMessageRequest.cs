// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Lib.Net.Http.WebPush;

namespace CleanAspire.Api.PushNotifications;

public class PushMessageRequest
{
    public required string Topic { get; set; }

    public required string Notification { get; set; }

    public PushMessageUrgency Urgency { get; set; } = PushMessageUrgency.Normal;
}
