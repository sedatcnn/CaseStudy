using System.Text;
using System.Text.Json;
using ProductService.Application.Interfaces;
using RabbitMQ.Client;

namespace ProductService.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private const string ExchangeName = "product_events";

    public RabbitMqEventPublisher(IConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync<T>(T @event, string routingKey, CancellationToken ct = default)
        where T : class
    {
        // 1. Kanal oluştur (Mesajın geçeceği yol)
        using var channel = await _connection.CreateChannelAsync();

        // 2. Exchange Tanımla (Gelen mesajları kim nereye dağıtacak?)
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        // 3. Mesajı JSON yap ve Byte dizisine çevir
        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        // 4. Mesajı fırlat (Publish)
        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            body: body);
    }
}