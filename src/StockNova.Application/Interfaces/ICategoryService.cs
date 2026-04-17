using StockNova.Application.DTOs.Categories;
using StockNova.Domain.Common;

namespace StockNova.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request);
    Task<Result<IReadOnlyList<CategoryResponse>>> GetAllAsync();
    Task<Result<CategoryResponse>> GetByIdAsync(int id);
}
