# ── Build Stage ──────────────────────────────────────────────────────────────
# 12 Faktör #5: Build ve run aşamaları birbirinden ayrıdır.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Önce sadece proje dosyaları — layer cache optimizasyonu
COPY . .


RUN dotnet publish src/Services/AuthService/AuthService.API/AuthService.API/AuthService.API.csproj -c Release -o /app/publish

# ── Runtime Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Güvenlik: root olmayan kullanıcı
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# 12 Faktör #7: Port ortam değişkeni ile belirlenir
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AuthService.API.dll"]
