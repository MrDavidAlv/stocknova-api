using StockNova.Application.DTOs.Common;
using StockNova.Application.DTOs.Products;
using StockNova.Domain.Common;

namespace StockNova.Application.Interfaces;

public interface IProductService
{
    Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request, int? userId = null);
    Task<Result<int>> BulkCreateAsync(BulkCreateRequest request);
    Task<Result<ImportResult>> ImportFromCsvAsync(Stream csvStream);
    Task<Result<PagedResult<ProductResponse>>> GetAllAsync(ProductFilterParams filterParams);
    Task<Result<ProductDetailResponse>> GetByIdAsync(int id);
    Task<Result<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request, int? userId = null);
    Task<Result> DeleteAsync(int id);
}
