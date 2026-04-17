using Microsoft.EntityFrameworkCore;
using StockNova.Domain.Entities;
using StockNova.Domain.Interfaces.Repositories;
using StockNova.Infrastructure.Data;

namespace StockNova.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .OrderBy(c => c.CategoryName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Categories.AddAsync(category, cancellationToken);
        return entry.Entity;
    }

    public async Task<bool> ExistsByNameAsync(string categoryName, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.CategoryName.ToLower() == categoryName.ToLower(), cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
