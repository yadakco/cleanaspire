// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Application.Features.Tenants.Commands;

public record DeleteTenantCommand(params IEnumerable<string> Ids) : IRequest;


public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand>
{
    private readonly ILogger<DeleteTenantCommandHandler> _logger;
    private readonly IApplicationDbContext _dbContext;

    public DeleteTenantCommandHandler(ILogger<DeleteTenantCommandHandler> logger, IApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        // Logic to delete tenants from the database
        _dbContext.Tenants.RemoveRange(_dbContext.Tenants.Where(t => request.Ids.Contains(t.Id)));
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Tenants with Ids {TenantIds} are deleted", string.Join(", ", request.Ids));
    }
}
