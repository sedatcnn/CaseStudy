namespace LogService.Domain.Entities;

/// <summary>
/// Merkezi log kaydı entity'si.
/// Structured Logging: Her alan ayrı bir kolon — Seq/ELK'da filtreleme kolaylaşır.
/// </summary>
public class LogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ServiceName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;     // INFO, WARNING, ERROR, CRITICAL
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public string? CorrelationId { get; set; }            // Dağıtık trace için
    public string? UserId { get; set; }
    public string? EventType { get; set; }                // ProductAdded, UserLoggedIn vb.
    public Dictionary<string, object>? Properties { get; set; }  // Ek metadata
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
