using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    /// <summary>
    /// Event yayıncısı (RabbitMQ) soyutlaması.
    /// Mesajlaşma altyapısı application katmanından gizlenir.
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event, string routingKey, CancellationToken ct = default) where T : class;
    }
}
