using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StockNova.Application.DTOs.Products;
using StockNova.Application.Interfaces;
using StockNova.Application.Mappings;
using StockNova.Application.Services;
using StockNova.Domain.Entities;
using StockNova.Domain.Interfaces.Repositories;
using StockNova.Domain.Specifications;

namespace StockNova.UnitTests.Application;

public class ProductServiceTests
{
    private readonly Mock<IProductReadRepository> _readRepoMock;
    private readonly Mock<IProductWriteRepository> _writeRepoMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly IMapper _mapper;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _readRepoMock = new Mock<IProductReadRepository>();
        _writeRepoMock = new Mock<IProductWriteRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _cacheMock = new Mock<ICacheService>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        var logger = new Mock<ILogger<ProductService>>();

        _service = new ProductService(
            _readRepoMock.Object,
            _writeRepoMock.Object,
            _categoryRepoMock.Object,
            _mapper,
            logger.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnSuccess()
    {
        var request = new CreateProductRequest
        {
            ProductName = "Test Product",
            CategoryId = 1,
            SupplierId = 1,
            UnitPrice = 99.99m,
            UnitsInStock = 10
        };

        var createdProduct = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            CategoryId = 1,
            SupplierId = 1,
            UnitPrice = 99.99m,
            UnitsInStock = 10
        };

        _readRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default))
            .ReturnsAsync(createdProduct);

        var result = await _service.CreateAsync(request, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ProductName.Should().Be("Test Product");
        _writeRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _writeRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
        _cacheMock.Verify(c => c.Remove("products_list"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            UnitPrice = 50m,
            Category = new Category { CategoryId = 1, CategoryName = "Servers" }
        };

        _cacheMock.Setup(c => c.Get<ProductDetailResponse>(It.IsAny<string>()))
            .Returns((ProductDetailResponse?)null);
        _readRepoMock.Setup(r => r.GetBySpecificationAsync(It.IsAny<ISpecification<Product>>(), default))
            .ReturnsAsync(product);

        var result = await _service.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ProductName.Should().Be("Test Product");
        _cacheMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<ProductDetailResponse>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCached_ShouldReturnFromCache()
    {
        var cached = new ProductDetailResponse { ProductId = 1, ProductName = "Cached" };
        _cacheMock.Setup(c => c.Get<ProductDetailResponse>("product_1")).Returns(cached);

        var result = await _service.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ProductName.Should().Be("Cached");
        _readRepoMock.Verify(r => r.GetBySpecificationAsync(It.IsAny<ISpecification<Product>>(), default), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        _cacheMock.Setup(c => c.Get<ProductDetailResponse>(It.IsAny<string>()))
            .Returns((ProductDetailResponse?)null);
        _readRepoMock.Setup(r => r.GetBySpecificationAsync(It.IsAny<ISpecification<Product>>(), default))
            .ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(999);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_ShouldReturnSuccess()
    {
        var existing = new Product { ProductId = 1, ProductName = "Old Name", UnitPrice = 10m };
        var request = new UpdateProductRequest
        {
            ProductName = "New Name",
            CategoryId = 1,
            UnitPrice = 20m,
            UnitsInStock = 5,
            Discontinued = false
        };

        _readRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(existing);

        var result = await _service.UpdateAsync(1, request, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ProductName.Should().Be("New Name");
        _writeRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), default), Times.Once);
        _cacheMock.Verify(c => c.Remove("product_1"), Times.Once);
        _cacheMock.Verify(c => c.Remove("products_list"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
    {
        _readRepoMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Product?)null);

        var result = await _service.UpdateAsync(999, new UpdateProductRequest { ProductName = "X" });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_ShouldSoftDelete()
    {
        var product = new Product { ProductId = 1, ProductName = "To Delete" };
        _readRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(product);

        var result = await _service.DeleteAsync(1);

        result.IsSuccess.Should().BeTrue();
        _writeRepoMock.Verify(r => r.SoftDeleteAsync(product, default), Times.Once);
        _cacheMock.Verify(c => c.Remove("product_1"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnFailure()
    {
        _readRepoMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Product?)null);

        var result = await _service.DeleteAsync(999);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResult()
    {
        var products = new List<Product>
        {
            new() { ProductId = 1, ProductName = "P1" },
            new() { ProductId = 2, ProductName = "P2" }
        };

        _readRepoMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<Product>>(), default))
            .ReturnsAsync(products);
        _readRepoMock.Setup(r => r.CountAsync(It.IsAny<ISpecification<Product>>(), default))
            .ReturnsAsync(2);

        var result = await _service.GetAllAsync(new ProductFilterParams { Page = 1, PageSize = 10 });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task BulkCreateAsync_WithInvalidCount_ShouldReturnFailure()
    {
        var result = await _service.BulkCreateAsync(new BulkCreateRequest { Count = 0 });
        result.IsSuccess.Should().BeFalse();

        var result2 = await _service.BulkCreateAsync(new BulkCreateRequest { Count = 600000 });
        result2.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task BulkCreateAsync_WithNoCategories_ShouldReturnFailure()
    {
        _categoryRepoMock.Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(new List<Category>());

        var result = await _service.BulkCreateAsync(new BulkCreateRequest { Count = 100 });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No categories");
    }

    [Fact]
    public async Task BulkCreateAsync_WithValidData_ShouldReturnCount()
    {
        _categoryRepoMock.Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(new List<Category> { new() { CategoryId = 1, CategoryName = "Test" } });

        var result = await _service.BulkCreateAsync(new BulkCreateRequest { Count = 100 });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(100);
        _writeRepoMock.Verify(r => r.BulkInsertAsync(It.IsAny<IEnumerable<Product>>(), default), Times.Once);
    }

    private static Stream CreateCsvStream(string content)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task ImportFromCsvAsync_WithValidCsv_ShouldImportProducts()
    {
        var csv = "ProductName,CategoryId,SupplierId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued\n" +
                  "Server Pro X1,1,2,1 unit,999.99,10,5,3,false\n" +
                  "Switch Elite 200,3,4,1 unit,650.00,20,0,5,false\n";

        using var stream = CreateCsvStream(csv);
        var result = await _service.ImportFromCsvAsync(stream);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Imported.Should().Be(2);
        result.Value.Failed.Should().Be(0);
        result.Value.TotalRows.Should().Be(2);
        _writeRepoMock.Verify(r => r.BulkInsertAsync(It.IsAny<IEnumerable<Product>>(), default), Times.Once);
    }

    [Fact]
    public async Task ImportFromCsvAsync_WithEmptyFile_ShouldReturnFailure()
    {
        using var stream = CreateCsvStream("");
        var result = await _service.ImportFromCsvAsync(stream);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task ImportFromCsvAsync_WithOnlyHeader_ShouldReturnFailure()
    {
        var csv = "ProductName,CategoryId,SupplierId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued\n";

        using var stream = CreateCsvStream(csv);
        var result = await _service.ImportFromCsvAsync(stream);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No valid products");
    }

    [Fact]
    public async Task ImportFromCsvAsync_WithInvalidRows_ShouldReportErrors()
    {
        var csv = "ProductName,CategoryId,SupplierId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued\n" +
                  "Valid Product,1,2,1 unit,100.00,10,0,5,false\n" +
                  "Bad Row,not_a_number\n" +
                  ",1,2,1 unit,50.00,5,0,3,false\n";

        using var stream = CreateCsvStream(csv);
        var result = await _service.ImportFromCsvAsync(stream);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Imported.Should().Be(1);
        result.Value.Failed.Should().Be(2);
        result.Value.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ImportFromCsvAsync_WithOptionalFieldsEmpty_ShouldImport()
    {
        var csv = "ProductName,CategoryId,SupplierId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued\n" +
                  "Minimal Product,,,,,,,, false\n";

        using var stream = CreateCsvStream(csv);
        var result = await _service.ImportFromCsvAsync(stream);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Imported.Should().Be(1);
    }
}
