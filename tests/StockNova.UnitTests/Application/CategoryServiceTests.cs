using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StockNova.Application.DTOs.Categories;
using StockNova.Application.Interfaces;
using StockNova.Application.Mappings;
using StockNova.Application.Services;
using StockNova.Domain.Entities;
using StockNova.Domain.Interfaces.Repositories;

namespace StockNova.UnitTests.Application;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly IMapper _mapper;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _cacheMock = new Mock<ICacheService>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        var logger = new Mock<ILogger<CategoryService>>();

        _service = new CategoryService(
            _categoryRepoMock.Object,
            _mapper,
            logger.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithNewName_ShouldReturnSuccess()
    {
        _categoryRepoMock.Setup(r => r.ExistsByNameAsync("Servers", default)).ReturnsAsync(false);

        var result = await _service.CreateAsync(new CreateCategoryRequest { CategoryName = "Servers" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.CategoryName.Should().Be("Servers");
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), default), Times.Once);
        _cacheMock.Verify(c => c.Remove("categories_all"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldReturnFailure()
    {
        _categoryRepoMock.Setup(r => r.ExistsByNameAsync("Servers", default)).ReturnsAsync(true);

        var result = await _service.CreateAsync(new CreateCategoryRequest { CategoryName = "Servers" });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetAllAsync_WhenCached_ShouldReturnFromCache()
    {
        var cached = new List<CategoryResponse>
        {
            new() { CategoryId = 1, CategoryName = "Servers" }
        } as IReadOnlyList<CategoryResponse>;

        _cacheMock.Setup(c => c.Get<IReadOnlyList<CategoryResponse>>("categories_all")).Returns(cached);

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        _categoryRepoMock.Verify(r => r.GetAllAsync(default), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WhenNotCached_ShouldQueryAndCache()
    {
        _cacheMock.Setup(c => c.Get<IReadOnlyList<CategoryResponse>>("categories_all"))
            .Returns((IReadOnlyList<CategoryResponse>?)null);

        var categories = new List<Category>
        {
            new() { CategoryId = 1, CategoryName = "Servers" },
            new() { CategoryId = 2, CategoryName = "Cloud" }
        };
        _categoryRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(categories);

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        _cacheMock.Verify(c => c.Set("categories_all", It.IsAny<IReadOnlyList<CategoryResponse>>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnCategory()
    {
        var category = new Category { CategoryId = 1, CategoryName = "Servers" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(category);

        var result = await _service.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CategoryName.Should().Be("Servers");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Category?)null);

        var result = await _service.GetByIdAsync(999);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}
