namespace StockNova.Application.DTOs.AuditLogs;

public class AuditLogResponse
{
    public long Id { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? Message { get; set; }
    public string Level { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}
