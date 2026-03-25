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
git clone [https://github.com/sedatcnn/CaseStudy.git](https://github.com/sedatcnn/CaseStudy.git)
cd CaseStudy
git checkout test/v1.0.0