# KayraExport Microservices Case Study

> **Backend Developer — Değerlendirme Projesi**  
> .NET 9 · Onion Mimari · CQRS · JWT · Docker

---

## 📐 Mimari Genel Bakış (Hedeflenen Yapı)

Şu an projenin ilk aşaması olan **Auth Service** tamamlanmıştır. Sistem tamamlandığında aşağıdaki yapıda olacaktır:
```
                        ┌─────────────────────────────────────────┐
Internet ──────────────►│          API Gateway (YARP)             │
                        │   Rate Limiting · Routing · CORS        │
                        └──────┬──────────┬──────────┬────────────┘
                               │          │          │
                    ┌──────────▼──┐  ┌────▼────┐  ┌─▼──────────┐
                    │ AuthService │  │Product  │  │ LogService  │
                    │  JWT + MS   │  │Service  │  │  Serilog +  │
                    │  Identity   │  │CQRS+    │  │  MongoDB    │
                    └──────┬──────┘  │Redis    │  └─────────────┘
                           │         └────┬────┘        ▲
                           │              │ RabbitMQ     │
                    ┌──────▼──────┐       └──────────────┘
                    │  SQL Server │
                    │  (AuthDb)   │
                    └─────────────┘
```
Her servis **Onion Mimari** ile katmanlıdır:
```
┌─────────────────────────────┐
│         API Layer           │  ← Controllers, Middleware, Program.cs
├─────────────────────────────┤
│     Application Layer       │  ← Commands, Queries, Handlers (MediatR/CQRS)
├─────────────────────────────┤
│       Domain Layer          │  ← Entities, Value Objects (sıfır dış bağımlılık)
├─────────────────────────────┤
│   Infrastructure Layer      │  ← EF Core, Redis, RabbitMQ, JWT
└─────────────────────────────┘
```

### Mevcut Durum: Auth Service
- **Kimlik Doğrulama:** Microsoft Identity framework kullanılarak kuruldu.
- **Yetkilendirme:** Role-based (Admin, User, Manager) JWT Bearer Token.
- **Veritabanı:** MS SQL Server (Docker üzerinde).
- **Mimari:** Onion Architecture (Domain, Application, Infrastructure, API).

---

## 🛠️ Teknolojiler (Auth Service)

| Katman | Teknoloji |
|--------|-----------|
| Framework | .NET 9 / ASP.NET Core |
| ORM | Entity Framework Core 9 |
| Auth | Microsoft Identity + JWT Bearer |
| CQRS | MediatR |
| Containerization | Docker + Docker Compose |

---

## 🏗️ Tasarım Kararları & Patternler

### 1. CQRS (Command Query Responsibility Segregation)
Kayıt (`RegisterCommand`) ve Giriş (`LoginCommand`) işlemleri MediatR kütüphanesi kullanılarak birbirinden ayrılmıştır. 
**Neden:** Okuma ve yazma modellerinin ayrılması, ileride yapılacak genişletmelerde (örneğin Read DB'nin ayrılması) kodun bozulmasını engeller.

### 2. Onion Architecture
Bağımlılıklar içe doğrudur. Domain katmanı hiçbir dış kütüphaneye bağımlı değildir. 
**Neden:** İş mantığını (Business Logic) dış dünyadan (DB, Framework) izole ederek test edilebilirliği artırmak.

---

## 🚀 Kurulum & Çalıştırma (Hızlı Başlangıç)

### 1. Repo'yu Klonla ve Branch'e Geç
```bash
git clone https://github.com/sedatcnn/CaseStudy.git
cd CaseStudy
git checkout test/v1.0.0
```

---

## 📦 Genişletilmiş Servis Yapısı (Product & Log Services)

Projenin ikinci aşamasında sisteme **ProductService** ve **LogService** dahil edilerek servisler arası asenkron iletişim (Event-Driven) altyapısı kurulmuştur.

### 1. Product Service (Katalog Yönetimi)

- **Teknolojiler:** .NET 9, EF Core (MSSQL), StackExchange.Redis, RabbitMQ
- **Performans:** Ürün listeleme işlemlerinde Redis Cache mekanizması entegre edilmiştir
- **Asenkron İletişim:** Yeni bir ürün eklendiğinde veya güncellendiğinde RabbitMQ üzerinden `product.added` veya `product.updated` event'leri yayınlanır
- **Güvenlik:** Role-based Authorization (`Admin`, `Manager`) ile korunmaktadır

### 2. Log Service (Merkezi Loglama)

- **Teknolojiler:** .NET 9, EF Core (MSSQL), RabbitMQ Consumer
- **Mimari:** Event-Driven Consumer yapısı
- **İşleyiş:** `BackgroundService` (IHostedService) aracılığıyla RabbitMQ kuyruğunu (`log_queue`) sürekli dinler
- **Depolama:** Dağıtık sistemden gelen tüm event'leri merkezi bir MSSQL (LogDb) tablosuna asenkron olarak işler
- **İzlenebilirlik:** `CorrelationId` desteği ile bir işlemin tüm mikroservislerdeki ayak izi takip edilebilir

---

## 🛠️ Güncellenmiş Teknoloji Matrisi

| Servis | Veritabanı | Messaging / Cache | Auth / Security |
|--------|------------|-------------------|-----------------|
| **AuthService** | MS SQL (AuthDb) | - | Identity + JWT |
| **ProductService** | MS SQL (ProductDb) | Redis + RabbitMQ | Bearer Token |
| **LogService** | MS SQL (LogDb) | RabbitMQ Consumer | Admin-Only Access |

---

## 🔄 Servisler Arası Akış (Event-Driven Design)
```
1. İstek      → Kullanıcı ProductService üzerinden yeni bir ürün oluşturur
                ↓
2. Persistence → Ürün ProductDb'ye kaydedilir, Redis cache anahtarı temizlenir
                ↓
3. Publish     → ProductService, RabbitMQ Exchange'ine ProductAddedEvent fırlatır
                ↓
4. Consume     → LogService arka planda bu mesajı yakalar
                ↓
5. Logging     → Mesaj içeriği, LogDb içerisine structured log olarak kaydedilir
```

---

## 🐳 Docker Compose Yapılandırması (Geliştirme Ortamı)

Tüm sistem tek bir komutla izole network üzerinde ayağa kalkacak şekilde optimize edilmiştir:

### Edge Portlar (Servisler)
- **Auth Service:** `5001`
- **Product Service:** `5002`
- **Log Service:** `5003`

### Infrastructure Portlar
- **MSSQL:** `1433`
- **Redis:** `6380` (Local çakışma önleyici)
- **RabbitMQ AMQP:** `5673`
- **RabbitMQ Management UI:** `15673`

### Tüm Sistemi Ayağa Kaldırmak
```bash
# Docker konteynerlerini build edip başlat
docker-compose up --build -d

# Logları izle
docker-compose logs -f

# Sistemi durdur
docker-compose down

# Veritabanı dahil tüm verileri temizle
docker-compose down -v
```

---

## 📑 API Dokümantasyonu (Swagger)

Her servis kendi Swagger arayüzüne sahiptir. Geliştirme aşamasında aşağıdaki adreslerden erişilebilir:

- **Auth API:** `http://localhost:5001/swagger`
- **Product API:** `http://localhost:5002/swagger`
- **Log API:** `http://localhost:5003/swagger`

### Swagger Kullanımı (JWT Authentication)

1. **Auth Service**'de (`/swagger`) Login endpoint'ini kullanarak token alın
2. Dönen `accessToken` değerini kopyalayın
3. Diğer servislerde (Product/Log) **Authorize** butonuna tıklayın
4. Token'ı yapıştırın (sadece token'ı, "Bearer" kelimesi otomatik eklenecek)
5. Korumalı endpoint'leri test edin

---

## 🛡️ Güvenlik Notları

### JWT Claim Mapping Standardizasyonu

Microsoft Identity claim tiplerinin (`Role`, `NameIdentifier`) mikroservisler arasında sorunsuz çözümlenmesi için aşağıdaki yaklaşım kullanılmıştır:
```csharp
// Product Service ve diğer consumer servislerde
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
```

Bu sayede:
- Auth Service → Token'a `"role": "Admin"` şeklinde kısa claim ekler
- Product Service → `RoleClaimType = "role"` ile aynı formatta okur
- Claim dönüşümü sorunu ortadan kalkar

### Role-Based Authorization

| Rol | Açıklama | Erişim |
|-----|----------|--------|
| **Admin** | Sistem yöneticisi | Tüm endpoint'ler (CRUD + Delete) |
| **Manager** | Ürün yöneticisi | Ürün ekleme/güncelleme |
| **User** | Standart kullanıcı | Sadece okuma işlemleri |

### Policy-Based Auth Örnekleri
```csharp
// Sadece Admin erişebilir
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteProduct(Guid id) { }

// Admin veya Manager erişebilir
[Authorize(Roles = "Admin,Manager")]
public async Task<IActionResult> AddProduct([FromBody] AddProductRequest request) { }

// Herkes erişebilir
[AllowAnonymous]
public async Task<IActionResult> GetProducts() { }
```
---
## 🧪 Test Senaryosu (End-to-End)

### Adım 1: Kullanıcı Kaydı ve Giriş
```bash
# 1. Register (Auth Service)
curl -X POST http://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@test.com",
    "password": "Admin@123456",
    "firstName": "Test",
    "lastName": "Admin"
  }'

# 2. Login
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@test.com",
    "password": "Admin@123456"
  }'
```

### Adım 2: Ürün İşlemleri
```bash
# Token'ı değişkene ata
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# 3. Ürün Ekle (Product Service)
curl -X POST http://localhost:5002/api/v1/products \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "iPhone 15 Pro",
    "description": "256GB Space Black",
    "price": 49999.99,
    "stock": 50,
    "category": "Electronics"
  }'

# 4. Ürünleri Listele (Cache'den gelecek)
curl -X GET http://localhost:5002/api/v1/products \
  -H "Authorization: Bearer $TOKEN"
```

### Adım 3: Log Kontrolü
```bash
# 5. Logları Görüntüle (Log Service - Admin Only)
curl -X GET http://localhost:5003/api/v1/logs \
  -H "Authorization: Bearer $TOKEN"
```

---

## 🔧 Sorun Giderme (Troubleshooting)

### Docker Container Ayağa Kalkmıyor
```bash
# Container durumunu kontrol et
docker ps -a

# Logları incele
docker logs kayra-auth-service
docker logs kayra-product-service
docker logs kayra-sqlserver

# Tüm container'ları durdur ve yeniden başlat
docker-compose down
docker-compose up --build
```

### 401 Unauthorized Hatası - ✅ ÇÖZÜLDÜ

**Sorun:** Microsoft Identity'nin uzun URI claim formatı (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`) ile JWT Bearer middleware'inin kısa claim formatı arasında uyumsuzluk.

**Çözüm:**

1. **Auth Service** token oluştururken kısa format kullanır:
```csharp
claims.Add(new Claim("role", role));  // "role" - kısa format
```

2. **Product Service** token okurken aynı formatı bekler:
```csharp
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();  // Microsoft'un otomatik dönüşümünü kapat
opt.TokenValidationParameters = new TokenValidationParameters
{
    RoleClaimType = "role",  // Kısa format
    NameClaimType = "sub"
};
```

3. **Swagger yapılandırması** düzeltildi:
```csharp
Type = SecuritySchemeType.Http,  // ApiKey yerine Http
Scheme = "Bearer"  // Otomatik "Bearer " prefix ekler
```

**Sonuç:** Servisler arası JWT authentication sorunsuz çalışıyor.

---

## 🌐 API Gateway (YARP) Entegrasyonu

Projenin son aşamasında **YARP (Yet Another Reverse Proxy)** kullanılarak API Gateway katmanı eklenmiştir.

### Özellikler

- **Unified Entry Point:** Tüm servislere tek bir endpoint (`http://localhost:5000`) üzerinden erişim
- **Dynamic Routing:** Gelen istekleri ilgili mikroservise yönlendirir
- **Load Balancing:** Gelecekte horizontal scaling için hazır altyapı
- **CORS Yönetimi:** Merkezi CORS politikası

### Gateway Routing Tablosu

| Path | Hedef Servis | Port |
|------|--------------|------|
| `/auth/**` | Auth Service | 5001 |
| `/products/**` | Product Service | 5002 |
| `/logs/**` | Log Service | 5003 |

### Kullanım Örneği
```bash
# Eski yöntem (direkt servis)
curl http://localhost:5001/api/v1/auth/login

# Yeni yöntem (Gateway üzerinden)
curl http://localhost:5000/auth/api/v1/auth/login
```
---
