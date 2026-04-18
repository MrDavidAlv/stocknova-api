namespace StockNova.Infrastructure.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Name { get; set; } = "stocknova_db";
    public string Username { get; set; } = "stocknova";
}
