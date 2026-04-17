using Microsoft.EntityFrameworkCore;
using StockNova.Domain.Entities;
using StockNova.Domain.Interfaces.Repositories;
using StockNova.Domain.Specifications;
using StockNova.Infrastructure.Data;

namespace StockNova.Infrastructure.Repositories;

public class ProductReadRepository : IProductReadRepository
{
    private readonly ApplicationDbContext _context;

    public ProductReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);
    }

    public async Task<Product?> GetBySpecificationAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default)
    {
        return await _context.Products.Where(specification.Criteria).CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.ProductId == id, cancellationToken);
    }

    private IQueryable<Product> ApplySpecification(ISpecification<Product> spec)
    {
        var query = _context.Products.Where(spec.Criteria);

        foreach (var include in spec.Includes)
        {
            query = query.Include(include);
        }

        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        if (spec.Skip.HasValue)
        {
            query = query.Skip(spec.Skip.Value);
        }

        if (spec.Take.HasValue)
        {
            query = query.Take(spec.Take.Value);
        }

        return query;
    }
}
