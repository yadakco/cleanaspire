// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;
public class Stock : BaseAuditableEntity, IAuditTrial
{
    public string? ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public string Location { get; set; }=string.Empty;
}
