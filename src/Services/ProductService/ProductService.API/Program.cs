using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Cache;
using ProductService.Infrastructure.Messaging;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var builder = WebApplication.CreateBuilder(args);

// --- Veritabanı ---
builder.Services.AddDbContext<ProductDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Redis ---
var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddStackExchangeRedisCache(opt => opt.Configuration = redisConn);
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// --- RabbitMQ ---
builder.Services.AddSingleton<IConnection>(_ => {
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost"
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

// --- App Services & MediatR ---
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProductService.Application.Interfaces.ICacheService).Assembly));

// --- Auth & JWT ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(opt => {
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Şimdilik kapat, uyuşmazlık olabilir
        ValidateAudience = false, // Şimdilik kapat
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
        
        // .NET'e diyoruz ki: "Mapping falan yapma, ne geliyorsa onu kullan"
        //RoleClaimType = "role",
        //NameClaimType = "sub"
    };
    
    // EK OLARAK: Gateway üzerinden gelince bazen header kaybolur, bunu zorlayalım
    opt.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context => {
            Console.WriteLine("Auth Failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context => {
            Console.WriteLine("Token Validated Successfully!");
            return Task.CompletedTask;
        }
    };
});

// BURASI DA ÇOK ÖNEMLİ: Policy adını ve rollerini temizle
builder.Services.AddAuthorization(opt => {
    opt.AddPolicy("ManagerOrAdmin", p => p.RequireRole("Admin", "Manager"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Service API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // ApiKey yerine Http yapıyoruz
        Scheme = "Bearer",             // Şema adını direkt Bearer veriyoruz
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Sadece Access Token değerini yapıştırın (Bearer yazmanıza gerek yok)."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
var app = builder.Build();

// Auto-Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.Use(async (context, next) =>
{
    string authHeader = context.Request.Headers["Authorization"];
    if (!string.IsNullOrEmpty(authHeader))
    {
        // Eğer Bearer kelimesi eksikse biz ekliyoruz
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Headers["Authorization"] = "Bearer " + authHeader;
        }
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription(); app.Run();