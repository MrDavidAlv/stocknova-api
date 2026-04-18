namespace StockNova.Infrastructure.Configuration;

public class SeedUserOptions
{
    public const string SectionName = "SeedUsers";

    public string? Admin { get; set; }
    public string? Manager { get; set; }
    public string? Viewer { get; set; }
}
