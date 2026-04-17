using StockNova.Domain.Common;

namespace StockNova.Domain.Entities;

public class Product : AuditableEntity
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public int? CategoryId { get; set; }
    public string? QuantityPerUnit { get; set; }
    public decimal? UnitPrice { get; set; }
    public short? UnitsInStock { get; set; }
    public short? UnitsOnOrder { get; set; }
    public short? ReorderLevel { get; set; }
    public bool Discontinued { get; set; }

    // Navigation
    public Supplier? Supplier { get; set; }
    public Category? Category { get; set; }
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    // Factory method
    public static Product Create(
        string productName,
        int? categoryId,
        int? supplierId,
        decimal? unitPrice,
        short? unitsInStock,
        int? createdBy = null)
    {
        return new Product
        {
            ProductName = productName,
            CategoryId = categoryId,
            SupplierId = supplierId,
            UnitPrice = unitPrice,
            UnitsInStock = unitsInStock,
            Discontinued = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}
