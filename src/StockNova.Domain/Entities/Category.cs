using StockNova.Domain.Common;

namespace StockNova.Domain.Entities;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public byte[]? Picture { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
