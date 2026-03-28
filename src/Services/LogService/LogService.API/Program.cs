using LogService.Application.Interfaces;
using LogService.Infrastructure.Consumers;
using LogService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using System.Text;

// ─── Serilog Yapılandırması ────────────────────────────────────────────────────
// 12 Faktör #11: Loglar stdout'a yazılır → container log driver toplar
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    // Seq — merkezi log sunucusu (isteğe bağlı)
    .WriteTo.Seq(
        serverUrl: Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://seq:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();  // ASP.NET Core loglarını Serilog'a yönlendir

// 1. Veritabanı (MSSQL)
builder.Services.AddDbContext<LogDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILogRepository, SqlLogRepository>();

// 2. RabbitMQ Bağlantısı
builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq",
        UserName = builder.Configuration["RabbitMQ:Username"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Password"] ?? "guest",
        AutomaticRecoveryEnabled = true
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});
// 3. Arka Plan Dinleyicisi
builder.Services.AddHostedService<ProductEventConsumer>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(opt => {
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),

        // AGA BURAYI BÖYLE YAP: Link yazmak yerine ClaimTypes kullan
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
    };
});
builder.Services.AddAuthorization(opt =>
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Bunu bul ve değiştir:
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Log Service API", Version = "v1" });

    // --- BU KISIM AUTHORIZE BUTONUNU GETİRİR ---
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Token giriniz. Örnek: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
}); builder.Services.AddHealthChecks();
var app = builder.Build();

// if bloğunu komple sil, şu iki satır kalsın:
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LogService API v1");
});
app.UseSerilogRequestLogging(); // HTTP istek logları
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("[LogService] Graceful shutdown — Serilog flush başlatıldı.");
    Log.CloseAndFlush();
});
app.Run();