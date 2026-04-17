using AutoMapper;
using Microsoft.Extensions.Logging;
using StockNova.Application.DTOs.Categories;
using StockNova.Application.Interfaces;
using StockNova.Domain.Common;
using StockNova.Domain.Entities;
using StockNova.Domain.Interfaces.Repositories;

namespace StockNova.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;
    private readonly ICacheService _cacheService;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<CategoryService> logger,
        ICacheService cacheService)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request)
    {
        if (await _categoryRepository.ExistsByNameAsync(request.CategoryName))
        {
            return Result<CategoryResponse>.Failure($"Category '{request.CategoryName}' already exists");
        }

        var category = _mapper.Map<Category>(request);
        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        _cacheService.Remove("categories_all");
        _logger.LogInformation("Category created: {CategoryId} - {CategoryName}", category.CategoryId, category.CategoryName);

        return Result<CategoryResponse>.Success(_mapper.Map<CategoryResponse>(category));
    }

    public async Task<Result<IReadOnlyList<CategoryResponse>>> GetAllAsync()
    {
        var cacheKey = "categories_all";
        var cached = _cacheService.Get<IReadOnlyList<CategoryResponse>>(cacheKey);
        if (cached != null)
        {
            return Result<IReadOnlyList<CategoryResponse>>.Success(cached);
        }

        var categories = await _categoryRepository.GetAllAsync();
        var response = _mapper.Map<IReadOnlyList<CategoryResponse>>(categories);

        _cacheService.Set(cacheKey, response, TimeSpan.FromMinutes(10));
        return Result<IReadOnlyList<CategoryResponse>>.Success(response);
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result<CategoryResponse>.Failure($"Category with ID {id} not found");
        }

        return Result<CategoryResponse>.Success(_mapper.Map<CategoryResponse>(category));
    }
}
