using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanAspire.Infrastructure.Persistence.Seed;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context,
      UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }
    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
                await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }
    public async Task SeedAsync()
    {
        try
        {
            await SeedUsersAsync();
            await SeedDataAsync();
            _context.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
    private async Task SeedUsersAsync()
    {
        if (!(await _context.Tenants.AnyAsync()))
        {
            var tenants = new List<Tenant>()
            {
                new()
                {
                    Name = "Org - 1",
                    Description = "Organization 1",
                    Id = Guid.CreateVersion7().ToString()
                },
                new()
                {
                    Name = "Org - 2",
                    Description = "Organization 2",
                    Id = Guid.CreateVersion7().ToString()
                }
            };
            _context.Tenants.AddRange(tenants);
            await _context.SaveChangesAsync();
        }

        if (await _userManager.Users.AnyAsync()) return;
        var tenantId = _context.Tenants.First().Id;
        var defaultPassword = "P@ssw0rd!";
        _logger.LogInformation("Seeding users...");
        var adminUser = new ApplicationUser
        {
            UserName = "Administrator",
            Provider = "Local",
            TenantId = tenantId,
            Nickname = "Administrator",
            Email = "admin@example.com",
            EmailConfirmed = true,
            LanguageCode = "en-US",
            TimeZoneId = "Asia/Shanghai",
            TwoFactorEnabled = false
        };

        var demoUser = new ApplicationUser
        {
            UserName = "Demo",
            Provider = "Local",
            TenantId = tenantId,
            Nickname = "Demo",
            Email = "Demo@example.com",
            EmailConfirmed = true,
            LanguageCode = "en-US",
            TimeZoneId = "Asia/Shanghai",
            TwoFactorEnabled = false,
            SuperiorId = adminUser.Id

        };

        await _userManager.CreateAsync(adminUser, defaultPassword);
        await _userManager.CreateAsync(demoUser, defaultPassword);
    }
    private async Task SeedDataAsync()
    {
        if (await _context.Products.AnyAsync()) return;
        _logger.LogInformation("Seeding data...");
        var products = new List<Product>
{
    new Product
    {
        Name = "Sony Bravia 65-inch 4K TV",
        Description = "Sony's 65-inch Bravia 4K Ultra HD smart TV with HDR support and X-Motion Clarity. Features a slim bezel, Dolby Vision, and an immersive sound system. Perfect for high-definition streaming and gaming.",
        Price = 1200,
        Quantity = 30,
        UOM = "PCS",
        Currency = "USD"
    },
    new Product
    {
        Name = "Tesla Model S Plaid",
        Description = "Tesla's flagship electric vehicle with a top speed of 200 mph and 0-60 in under 2 seconds. Equipped with Autopilot, long-range battery, and premium interior. Suitable for eco-conscious luxury seekers.",
        Price = 120000,
        Quantity = 5,
        UOM = "PCS",
        Currency = "USD"
    },
    new Product
    {
        Name = "Apple iPhone 14 Pro Max",
        Description = "Apple's latest iPhone featuring a 6.7-inch OLED display, A16 Bionic chip, advanced camera system with 48 MP main camera, and longer battery life. Ideal for photography and heavy app users.",
        Price = 1099,
        Quantity = 150,
        UOM = "PCS",
        Currency = "USD"
    },
    new Product
    {
        Name = "Sony WH-1000XM5 Noise Cancelling Headphones",
        Description = "Premium noise-cancelling over-ear headphones with 30-hour battery life, adaptive sound control, and Hi-Res audio support. Designed for frequent travelers and audiophiles seeking uninterrupted sound.",
        Price = 349,
        Quantity = 200,
        UOM = "PCS",
        Currency = "USD"
    },
    new Product
    {
        Name = "Apple MacBook Pro 16-inch M2 Max",
        Description = "Apple’s most powerful laptop featuring the M2 Max chip, a stunning 16-inch Liquid Retina XDR display, 64GB of unified memory, and up to 8TB SSD storage. Ideal for creative professionals needing high performance.",
        Price = 4200,
        Quantity = 15,
        UOM = "PCS",
        Currency = "USD"
    }
};
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
    }
}

