using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Seeders;

public static class ShipperSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Shippers.AnyAsync())
        {
            return;
        }

        var shippers = new List<Shipper>
        {
            new() { CompanyName = "FedEx", Phone = "+1-800-463-3339" },
            new() { CompanyName = "DHL Express", Phone = "+1-800-225-5345" },
            new() { CompanyName = "UPS", Phone = "+1-800-742-5877" }
        };

        await context.Shippers.AddRangeAsync(shippers);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} shippers", shippers.Count);
    }
}
