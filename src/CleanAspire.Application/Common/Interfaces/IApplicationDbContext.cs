using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; set; }
    DbSet<AuditTrail> AuditTrails { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
}

