namespace StockNova.Application.DTOs.Products;

public class CreateProductRequest
{
    public string ProductName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public int? CategoryId { get; set; }
    public string? QuantityPerUnit { get; set; }
    public decimal? UnitPrice { get; set; }
    public short? UnitsInStock { get; set; }
    public short? UnitsOnOrder { get; set; }
    public short? ReorderLevel { get; set; }
    public bool Discontinued { get; set; }
}

public class UpdateProductRequest
{
    public string ProductName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public int? CategoryId { get; set; }
    public string? QuantityPerUnit { get; set; }
    public decimal? UnitPrice { get; set; }
    public short? UnitsInStock { get; set; }
    public short? UnitsOnOrder { get; set; }
    public short? ReorderLevel { get; set; }
    public bool Discontinued { get; set; }
}

public class BulkCreateRequest
{
    public int Count { get; set; } = 1000;
    public int? CategoryId { get; set; }
}

public class ImportResult
{
    public int TotalRows { get; set; }
    public int Imported { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
    public long ElapsedMs { get; set; }
}

public class ProductFilterParams
{
    public int? ProductId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? Discontinued { get; set; }
    public string SortBy { get; set; } = "ProductName";
    public string SortOrder { get; set; } = "asc";
}
