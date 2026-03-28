using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogService.Domain.Entities;

/// <summary>
/// Merkezi log kaydı entity'si.
/// Structured Logging: Her alan ayrı bir kolon — Seq/ELK'da filtreleme kolaylaşır.
/// </summary>
public class LogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ServiceName { get; set; } = "ProductService";
    public string Level { get; set; } = "INFO"; // INFO, ERROR
    public string Message { get; set; } = string.Empty;
    public string? EventType { get; set; } // ProductAdded vb.
    public string? CorrelationId { get; set; } // İz sürmek için
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

