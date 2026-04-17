namespace StockNova.Domain.Entities;

public class Shipper
{
    public int ShipperId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
