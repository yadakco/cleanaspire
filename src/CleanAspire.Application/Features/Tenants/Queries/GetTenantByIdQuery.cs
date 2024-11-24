// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Tenants.DTOs;
using MediatR;

namespace CleanAspire.Application.Features.Tenants.Queries;

public record GetTenantByIdQuery(string Id) : IRequest<TenantDto?>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTenantByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { request.Id }, cancellationToken);
        if (tenant == null)
        {
            return null; // or throw an exception if preferred
        }
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Description = tenant.Description
        };
    }
}
