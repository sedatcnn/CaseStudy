using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ─── YARP Reverse Proxy ───────────────────────────────────────────────────────
// YARP (Yet Another Reverse Proxy): Microsoft'un .NET'e özel gateway kütüphanesi.
// Tüm route/cluster konfigürasyonu appsettings.json'dan okunur → 12 Faktör #3.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(opt =>
{
    // Kapıdan dakikada en fazla 100 kişi geçebilsin (Sistemi yormamak için)
    opt.AddFixedWindowLimiter("default-limiter", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 100;
    });
});
// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opt =>
    opt.AddPolicy("AllowAll", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()));

// ─── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();


app.UseRouting(); // Rotaları belirle
app.UseCors("AllowAll"); // Varsa CORS'u aç

app.MapReverseProxy();
app.MapHealthChecks("/health");

app.Run();
