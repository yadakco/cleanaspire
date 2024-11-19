// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace CleanAspire.Application.Common.Interfaces;

public interface IUploadService
{
    Task<string> UploadAsync(UploadRequest request);
    void Remove(string filename);
}
public record UploadRequest(
    string FileName,
    UploadType UploadType,
    byte[] Data,
    bool Overwrite = false,
    string? Extension = null,
    string? Folder = null
);
public enum UploadType : byte
{
    [Description(@"Products")] Product,
    [Description(@"ProfilePictures")] ProfilePicture,
    [Description(@"Documents")] Document
}
