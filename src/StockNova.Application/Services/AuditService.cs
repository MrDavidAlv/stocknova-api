using AutoMapper;
using Microsoft.Extensions.Logging;
using StockNova.Application.DTOs.AuditLogs;
using StockNova.Application.DTOs.Common;
using StockNova.Application.Interfaces;
using StockNova.Domain.Common;
using StockNova.Domain.Entities;
using StockNova.Domain.Interfaces.Repositories;

namespace StockNova.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        IMapper mapper,
        ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task LogAsync(int? userId, string? userEmail, string action, string? entityName = null,
        string? entityId = null, string? oldValues = null, string? newValues = null,
        string? ipAddress = null, string? userAgent = null, string level = "Info",
        string? message = null, string? exceptionDetails = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Level = level,
                Message = message,
                ExceptionDetails = exceptionDetails,
                Timestamp = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for action: {Action}", action);
        }
    }

    public async Task<Result<PagedResult<AuditLogResponse>>> GetLogsAsync(
        DateTime? from = null, DateTime? to = null, string? action = null,
        string? entityName = null, int page = 1, int pageSize = 50)
    {
        var logs = await _auditLogRepository.GetLogsAsync(from, to, action, entityName, page, pageSize);
        var totalCount = await _auditLogRepository.CountAsync(from, to, action, entityName);

        var result = new PagedResult<AuditLogResponse>
        {
            Items = _mapper.Map<IReadOnlyList<AuditLogResponse>>(logs),
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<AuditLogResponse>>.Success(result);
    }
}
