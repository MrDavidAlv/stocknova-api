using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StockNova.Domain.Entities;
using StockNova.Domain.Enums;
using StockNova.Infrastructure.Configuration;

namespace StockNova.Infrastructure.Data.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, IConfiguration configuration)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var seedUserOptions = configuration.GetSection(SeedUserOptions.SectionName).Get<SeedUserOptions>()
            ?? new SeedUserOptions();
        var credentials = BuildSeedCredentials(seedUserOptions);

        var users = new List<User>
        {
            new()
            {
                Email = "admin@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(credentials.Admin),
                FullName = "Admin StockNova",
                Role = UserRole.Admin,
                IsActive = true
            },
            new()
            {
                Email = "manager@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(credentials.Manager),
                FullName = "Manager StockNova",
                Role = UserRole.Manager,
                IsActive = true
            },
            new()
            {
                Email = "viewer@stocknova.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(credentials.Viewer),
                FullName = "Viewer StockNova",
                Role = UserRole.Viewer,
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} default users", users.Count);
        logger.LogInformation("Seed user credentials were loaded from configuration");
    }

    private static (string Admin, string Manager, string Viewer) BuildSeedCredentials(SeedUserOptions options)
    {
        return (
            ResolveSeedSecret(options.Admin),
            ResolveSeedSecret(options.Manager),
            ResolveSeedSecret(options.Viewer)
        );
    }

    private static string ResolveSeedSecret(string? configuredValue)
    {
        return !string.IsNullOrWhiteSpace(configuredValue)
            ? configuredValue
            : Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
