namespace ProductService.Application.Events;

/// <summary>
/// Domain event'leri — RabbitMQ üzerinden yayınlanır.
/// Event-Driven Mimari: Servisler birbirini doğrudan çağırmaz,
/// event'ler üzerinden haberleşir → loose coupling sağlanır.
/// </summary>
public record ProductAddedEvent(
    Guid ProductId,
    string ProductName,
    decimal Price,
    string Category,
    string CreatedBy,
    DateTime OccurredAt);

public record ProductUpdatedEvent(
    Guid ProductId,
    string ProductName,
    decimal Price,
    string Category,
    string UpdatedBy,
    DateTime OccurredAt);

/// <summary>
/// SAGA pattern için kullanılır.
/// Dağıtık transaction'larda başarısızlık durumunda kompenzasyon işlemi tetikler.
/// </summary>
public record ProductOperationFailedEvent(
    Guid ProductId,
    string Operation,
    string Reason,
    DateTime OccurredAt);
