using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockNova.Domain.Entities;
using StockNova.Domain.Enums;
using StockNova.Domain.Interfaces.Services;

namespace StockNova.Infrastructure.Data.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // BCrypt hash generated inline for seed data
        var users = new List<User>
        {
            new()
            {
                Email = "admin@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FullName = "Admin StockNova",
                Role = UserRole.Admin,
                IsActive = true
            },
            new()
            {
                Email = "manager@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                FullName = "Manager StockNova",
                Role = UserRole.Manager,
                IsActive = true
            },
            new()
            {
                Email = "viewer@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Viewer123!"),
                FullName = "Viewer StockNova",
                Role = UserRole.Viewer,
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} users", users.Count);
    }
}
