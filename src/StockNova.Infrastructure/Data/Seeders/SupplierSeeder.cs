using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Seeders;

public static class SupplierSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Suppliers.AnyAsync())
        {
            return;
        }

        var suppliers = new List<Supplier>
        {
            new() { CompanyName = "Dell Technologies", ContactName = "Michael Dell", ContactTitle = "CEO", Country = "USA", City = "Round Rock", Phone = "+1-800-624-9897" },
            new() { CompanyName = "HP Enterprise", ContactName = "Antonio Neri", ContactTitle = "CEO", Country = "USA", City = "Houston", Phone = "+1-800-474-6836" },
            new() { CompanyName = "Lenovo", ContactName = "Yuanqing Yang", ContactTitle = "CEO", Country = "China", City = "Beijing", Phone = "+86-10-5886-8888" },
            new() { CompanyName = "Cisco Systems", ContactName = "Chuck Robbins", ContactTitle = "CEO", Country = "USA", City = "San Jose", Phone = "+1-800-553-6387" },
            new() { CompanyName = "Amazon Web Services", ContactName = "Adam Selipsky", ContactTitle = "CEO", Country = "USA", City = "Seattle", Phone = "+1-206-266-1000" }
        };

        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} suppliers", suppliers.Count);
    }
}
