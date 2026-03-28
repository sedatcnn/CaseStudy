using LogService.Domain.Entities;

namespace LogService.Application.Interfaces;

/// <summary>
/// Log deposu soyutlaması.
/// DIP: Application katmanı MongoDB/SQL detaylarını bilmez.
/// </summary>
public interface ILogRepository
{
    Task AddAsync(LogEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<LogEntry>> GetByServiceAsync(string serviceName,
        int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<LogEntry>> GetByLevelAsync(string level,
        int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<LogEntry>> GetByCorrelationIdAsync(string correlationId,
        CancellationToken ct = default);
}
