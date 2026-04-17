using StockNova.Domain.Entities;

namespace StockNova.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetLogsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? action = null,
        string? entityName = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? action = null,
        string? entityName = null,
        CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
