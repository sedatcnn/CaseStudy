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
        using var channel = await _connection.CreateChannelAsync();

        // Kuyruk tanımlama
        await channel.QueueDeclareAsync("log_queue", true, false, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());

            // Logu veritabanına kaydet
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ILogRepository>();
                await repo.AddAsync(new LogEntry
                {
                    Message = body,
                    ServiceName = "ProductService",
                    Level = "INFO",
                    OccurredAt = DateTime.UtcNow
                });
            }
            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await channel.BasicConsumeAsync("log_queue", false, consumer);
        await Task.Delay(-1, stoppingToken); // Durdurulana kadar bekle
    }
}