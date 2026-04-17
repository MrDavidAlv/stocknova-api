using FluentAssertions;
using StockNova.Domain.Entities;

namespace StockNova.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnProduct()
    {
        var product = Product.Create("Server Pro 100", 1, 2, 999.99m, 50, 1);

        product.ProductName.Should().Be("Server Pro 100");
        product.CategoryId.Should().Be(1);
        product.SupplierId.Should().Be(2);
        product.UnitPrice.Should().Be(999.99m);
        product.UnitsInStock.Should().Be(50);
        product.CreatedBy.Should().Be(1);
        product.Discontinued.Should().BeFalse();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNullOptionalFields_ShouldSetDefaults()
    {
        var product = Product.Create("Basic Product", null, null, null, null);

        product.ProductName.Should().Be("Basic Product");
        product.CategoryId.Should().BeNull();
        product.SupplierId.Should().BeNull();
        product.UnitPrice.Should().BeNull();
        product.UnitsInStock.Should().BeNull();
        product.CreatedBy.Should().BeNull();
        product.Discontinued.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldSetCreatedAtToUtcNow()
    {
        var before = DateTime.UtcNow;
        var product = Product.Create("Test", 1, 1, 10m, 5);
        var after = DateTime.UtcNow;

        product.CreatedAt.Should().BeOnOrAfter(before);
        product.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Product_DefaultValues_ShouldBeCorrect()
    {
        var product = new Product();

        product.ProductName.Should().BeEmpty();
        product.Discontinued.Should().BeFalse();
        product.IsDeleted.Should().BeFalse();
        product.OrderDetails.Should().BeEmpty();
    }
}
