using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using StockNova.Domain.Entities;
using StockNova.Domain.Interfaces.Repositories;
using StockNova.Infrastructure.Data;

namespace StockNova.Infrastructure.Repositories;

public class ProductWriteRepository : IProductWriteRepository
{
    private readonly ApplicationDbContext _context;

    public ProductWriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Products.AddAsync(product, cancellationToken);
        return entry.Entity;
    }

    public async Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddRangeAsync(products, cancellationToken);
    }

    public async Task BulkInsertAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        var productList = products.ToList();

        // Use EFCore.BulkExtensions for high-performance bulk insert
        var bulkConfig = new BulkConfig
        {
            SetOutputIdentity = false,
            BatchSize = 5000,
            UseTempDB = false
        };

        await _context.BulkInsertAsync(productList, bulkConfig, cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
