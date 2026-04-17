namespace StockNova.Application.DTOs.Products;

public class ProductResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? QuantityPerUnit { get; set; }
    public decimal? UnitPrice { get; set; }
    public short? UnitsInStock { get; set; }
    public short? UnitsOnOrder { get; set; }
    public short? ReorderLevel { get; set; }
    public bool Discontinued { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductDetailResponse : ProductResponse
{
    public byte[]? CategoryPicture { get; set; }
    public string? CategoryDescription { get; set; }
    public string? SupplierContactName { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierCountry { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
