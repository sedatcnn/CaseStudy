using LogService.Application.Interfaces;
using LogService.Infrastructure.Consumers;
using LogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı (MSSQL)
builder.Services.AddDbContext<LogDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILogRepository, SqlLogRepository>();

// 2. RabbitMQ Bağlantısı
builder.Services.AddSingleton<IConnection>(_ => {
    var factory = new ConnectionFactory { HostName = "rabbitmq" };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// 3. Arka Plan Dinleyicisi
builder.Services.AddHostedService<ProductEventConsumer>();

// 4. JWT Ayarları (ProductService ile aynı olsun)
// ... (Jwt kodların)

builder.Services.AddControllers();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();