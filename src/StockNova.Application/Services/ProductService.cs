using AutoMapper;
using Microsoft.Extensions.Logging;
using StockNova.Application.DTOs.Common;
using StockNova.Application.DTOs.Products;
using StockNova.Application.Interfaces;
using StockNova.Domain.Common;
using StockNova.Domain.Entities;
using StockNova.Domain.Interfaces.Repositories;
using StockNova.Domain.Specifications;

namespace StockNova.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductReadRepository _readRepository;
    private readonly IProductWriteRepository _writeRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    private readonly ICacheService _cacheService;

    public ProductService(
        IProductReadRepository readRepository,
        IProductWriteRepository writeRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<ProductService> logger,
        ICacheService cacheService)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request, int? userId = null)
    {
        var product = Product.Create(
            request.ProductName,
            request.CategoryId,
            request.SupplierId,
            request.UnitPrice,
            request.UnitsInStock,
            userId);

        product.QuantityPerUnit = request.QuantityPerUnit;
        product.UnitsOnOrder = request.UnitsOnOrder;
        product.ReorderLevel = request.ReorderLevel;
        product.Discontinued = request.Discontinued;

        await _writeRepository.AddAsync(product);
        await _writeRepository.SaveChangesAsync();

        _cacheService.Remove("products_list");
        _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.ProductId, product.ProductName);

        var createdProduct = await _readRepository.GetByIdAsync(product.ProductId);
        return Result<ProductResponse>.Success(_mapper.Map<ProductResponse>(createdProduct));
    }

    public async Task<Result<int>> BulkCreateAsync(BulkCreateRequest request)
    {
        if (request.Count <= 0 || request.Count > 500000)
        {
            return Result<int>.Failure("Count must be between 1 and 500,000");
        }

        var categories = await _categoryRepository.GetAllAsync();
        if (!categories.Any())
        {
            return Result<int>.Failure("No categories found. Create categories first.");
        }

        var categoryIds = request.CategoryId.HasValue
            ? new[] { request.CategoryId.Value }
            : categories.Select(c => c.CategoryId).ToArray();

        var random = new Random();
        var productNames = new[] { "Server", "Switch", "Router", "Firewall", "Storage", "Module", "Controller", "Adapter", "Cable", "Rack" };
        var brands = new[] { "Pro", "Elite", "Max", "Ultra", "Prime", "Core", "Edge", "Flex", "Nova", "Apex" };

        var products = Enumerable.Range(1, request.Count).Select(i =>
        {
            var name = $"{productNames[random.Next(productNames.Length)]} {brands[random.Next(brands.Length)]} {random.Next(100, 9999)}";
            return new Product
            {
                ProductName = name,
                CategoryId = categoryIds[random.Next(categoryIds.Length)],
                UnitPrice = Math.Round((decimal)(random.NextDouble() * 5000 + 10), 2),
                UnitsInStock = (short)random.Next(0, 1000),
                UnitsOnOrder = (short)random.Next(0, 100),
                ReorderLevel = (short)random.Next(5, 50),
                QuantityPerUnit = $"{random.Next(1, 24)} units",
                Discontinued = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }).ToList();

        _logger.LogInformation("Starting bulk insert of {Count} products", request.Count);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        await _writeRepository.BulkInsertAsync(products);

        sw.Stop();
        _logger.LogInformation("Bulk insert completed: {Count} products in {Elapsed}ms", request.Count, sw.ElapsedMilliseconds);

        _cacheService.Remove("products_list");
        return Result<int>.Success(request.Count);
    }

    public async Task<Result<PagedResult<ProductResponse>>> GetAllAsync(ProductFilterParams filterParams)
    {
        if (filterParams.PageSize > 50) filterParams.PageSize = 50;
        if (filterParams.Page < 1) filterParams.Page = 1;

        var spec = new ProductFilterSpecification(
            filterParams.Search,
            filterParams.CategoryId,
            filterParams.SupplierId,
            filterParams.MinPrice,
            filterParams.MaxPrice,
            filterParams.Discontinued,
            filterParams.Page,
            filterParams.PageSize,
            filterParams.SortBy,
            filterParams.SortOrder);

        var countSpec = new ProductFilterSpecification(
            filterParams.Search,
            filterParams.CategoryId,
            filterParams.SupplierId,
            filterParams.MinPrice,
            filterParams.MaxPrice,
            filterParams.Discontinued);

        var products = await _readRepository.ListAsync(spec);
        var totalCount = await _readRepository.CountAsync(countSpec);

        var result = new PagedResult<ProductResponse>
        {
            Items = _mapper.Map<IReadOnlyList<ProductResponse>>(products),
            CurrentPage = filterParams.Page,
            PageSize = filterParams.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<ProductResponse>>.Success(result);
    }

    public async Task<Result<ProductDetailResponse>> GetByIdAsync(int id)
    {
        var cacheKey = $"product_{id}";
        var cached = _cacheService.Get<ProductDetailResponse>(cacheKey);
        if (cached != null)
        {
            return Result<ProductDetailResponse>.Success(cached);
        }

        var spec = new ProductByIdSpecification(id);
        var product = await _readRepository.GetBySpecificationAsync(spec);

        if (product == null)
        {
            return Result<ProductDetailResponse>.Failure($"Product with ID {id} not found");
        }

        var response = _mapper.Map<ProductDetailResponse>(product);
        _cacheService.Set(cacheKey, response, TimeSpan.FromMinutes(5));

        return Result<ProductDetailResponse>.Success(response);
    }

    public async Task<Result<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request, int? userId = null)
    {
        var product = await _readRepository.GetByIdAsync(id);
        if (product == null)
        {
            return Result<ProductResponse>.Failure($"Product with ID {id} not found");
        }

        product.ProductName = request.ProductName;
        product.SupplierId = request.SupplierId;
        product.CategoryId = request.CategoryId;
        product.QuantityPerUnit = request.QuantityPerUnit;
        product.UnitPrice = request.UnitPrice;
        product.UnitsInStock = request.UnitsInStock;
        product.UnitsOnOrder = request.UnitsOnOrder;
        product.ReorderLevel = request.ReorderLevel;
        product.Discontinued = request.Discontinued;
        product.UpdatedBy = userId;
        product.UpdatedAt = DateTime.UtcNow;

        await _writeRepository.UpdateAsync(product);
        await _writeRepository.SaveChangesAsync();

        _cacheService.Remove($"product_{id}");
        _cacheService.Remove("products_list");
        _logger.LogInformation("Product updated: {ProductId}", id);

        return Result<ProductResponse>.Success(_mapper.Map<ProductResponse>(product));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var product = await _readRepository.GetByIdAsync(id);
        if (product == null)
        {
            return Result.Failure($"Product with ID {id} not found");
        }

        await _writeRepository.SoftDeleteAsync(product);
        await _writeRepository.SaveChangesAsync();

        _cacheService.Remove($"product_{id}");
        _cacheService.Remove("products_list");
        _logger.LogInformation("Product soft-deleted: {ProductId}", id);

        return Result.Success();
    }
}
