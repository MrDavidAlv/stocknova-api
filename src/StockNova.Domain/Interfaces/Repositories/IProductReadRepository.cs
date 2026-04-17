using StockNova.Domain.Entities;
using StockNova.Domain.Specifications;

namespace StockNova.Domain.Interfaces.Repositories;

public interface IProductReadRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySpecificationAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
