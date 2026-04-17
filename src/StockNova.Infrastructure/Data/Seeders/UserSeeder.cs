using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockNova.Domain.Entities;
using StockNova.Domain.Enums;
using StockNova.Domain.Interfaces.Services;

namespace StockNova.Infrastructure.Data.Seeders;

public static class UserSeeder
{
    // Default seed passwords — for development and testing only.
    // In production, change these immediately after first deploy.
    private const string DefaultAdminPassword = "Admin123!";
    private const string DefaultManagerPassword = "Manager123!";
    private const string DefaultViewerPassword = "Viewer123!";

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var users = new List<User>
        {
            new()
            {
                Email = "admin@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword),
                FullName = "Admin StockNova",
                Role = UserRole.Admin,
                IsActive = true
            },
            new()
            {
                Email = "manager@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultManagerPassword),
                FullName = "Manager StockNova",
                Role = UserRole.Manager,
                IsActive = true
            },
            new()
            {
                Email = "viewer@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultViewerPassword),
                FullName = "Viewer StockNova",
                Role = UserRole.Viewer,
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} default users", users.Count);
        logger.LogWarning("Default seed users created with default passwords. Change them in production");
    }
}
