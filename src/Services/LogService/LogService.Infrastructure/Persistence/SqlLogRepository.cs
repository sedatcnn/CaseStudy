using LogService.Application.Interfaces;
using LogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using LogService.Infrastructure.Persistence;

namespace LogService.Infrastructure.Persistence;

public class SqlLogRepository : ILogRepository
{
    private readonly LogDbContext _context;

    public SqlLogRepository(LogDbContext context)
    {
        _context = context;
    }

    // 1. Yeni Log Ekleme
    public async Task AddAsync(LogEntry entry, CancellationToken ct = default)
    {
        await _context.Logs.AddAsync(entry, ct);
        await _context.SaveChangesAsync(ct);
    }

    // 2. Servis Adına Göre Getirme (Zaten yazmıştık)
    public async Task<IReadOnlyList<LogEntry>> GetByServiceAsync(string serviceName, int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.Logs
            .Where(x => x.ServiceName == serviceName)
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    // --- ŞİMDİ EKSİK OLANLARI EKLİYORUZ ---

    // 3. Log Seviyesine Göre Getirme (INFO, ERROR vb.)
    public async Task<IReadOnlyList<LogEntry>> GetByLevelAsync(string level, int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.Logs
            .Where(x => x.Level == level.ToUpper()) // Küçük harf gelse bile büyük ara
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    // 4. CorrelationId'ye Göre Getirme (Hata takibi için)
    public async Task<IReadOnlyList<LogEntry>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default)
    {
        return await _context.Logs
            .Where(x => x.CorrelationId == correlationId)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(ct);
    }
}