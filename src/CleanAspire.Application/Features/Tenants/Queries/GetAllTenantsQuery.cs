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
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Tenants.Queries;

public record GetAllTenantsQuery() : IRequest<List<TenantDto>>;
public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, List<TenantDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllTenantsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TenantDto>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _dbContext.Tenants
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description
            })
            .ToListAsync(cancellationToken);

        return tenants;
    }
}
