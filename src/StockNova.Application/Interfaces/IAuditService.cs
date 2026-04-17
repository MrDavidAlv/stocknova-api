using StockNova.Application.DTOs.AuditLogs;
using StockNova.Application.DTOs.Common;
using StockNova.Domain.Common;

namespace StockNova.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(int? userId, string? userEmail, string action, string? entityName = null,
        string? entityId = null, string? oldValues = null, string? newValues = null,
        string? ipAddress = null, string? userAgent = null, string level = "Info",
        string? message = null, string? exceptionDetails = null);

    Task<Result<PagedResult<AuditLogResponse>>> GetLogsAsync(
        DateTime? from = null, DateTime? to = null, string? action = null,
        string? entityName = null, int page = 1, int pageSize = 50);
}
