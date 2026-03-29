using LogService.Application.Interfaces;
using LogService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace LogService.Infrastructure.Consumers;

// Infrastructure/Consumers/ProductEventConsumer.cs
public class ProductEventConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductEventConsumer(IConnection connection, IServiceScopeFactory scopeFactory)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync();

        // 1. Kargo Şubesini (Exchange) Tanımla (ProductService ile BİREBİR aynı olmalı)
        await channel.ExchangeDeclareAsync(
            exchange: "product_events",
            type: ExchangeType.Topic,
            durable: true);

        // 2. Posta Kutusunu (Queue) Tanımla
        await channel.QueueDeclareAsync(
            queue: "log_queue",
            durable: true,
            exclusive: false,
            autoDelete: false);

        // 3. İŞTE EKSİK OLAN ALTIN VURUŞ (Binding)
        // "product_events" şubesine gelen ve routing key'i "product.#" (product. ile başlayan her şey) olan mesajları log_queue'ya bağla!
        await channel.QueueBindAsync(
            queue: "log_queue",
            exchange: "product_events",
            routingKey: "product.#");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey; // Hangi event geldiğini anlamak için (örn: product.added)

            // Logu veritabanına kaydet
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ILogRepository>();
                await repo.AddAsync(new LogEntry
                {
                    Message = body,
                    ServiceName = "ProductService", // İstersen bunu routingKey'den de türetebilirsin
                    Level = "INFO",
                    OccurredAt = DateTime.UtcNow
                });
            }
            // Mesajı başarıyla işlediğimizi RabbitMQ'ya bildir (Kuyruktan silsin)
            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        // Dinlemeye başla
        await channel.BasicConsumeAsync(queue: "log_queue", autoAck: false, consumer: consumer);

        // Servis ayakta kaldığı sürece kanalı açık tut
        await Task.Delay(-1, stoppingToken);
    }
}