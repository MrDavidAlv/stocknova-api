using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Seeders;

public static class CategorySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { CategoryName = "SERVIDORES", Description = "Equipos de servidor y hardware de infraestructura" },
            new() { CategoryName = "CLOUD", Description = "Servicios y soluciones en la nube" },
            new() { CategoryName = "REDES", Description = "Equipos de redes y conectividad" },
            new() { CategoryName = "SOFTWARE", Description = "Licencias y soluciones de software" },
            new() { CategoryName = "ALMACENAMIENTO", Description = "Dispositivos y soluciones de almacenamiento" },
            new() { CategoryName = "SEGURIDAD", Description = "Productos y soluciones de seguridad informatica" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} categories", categories.Count);
    }
}
