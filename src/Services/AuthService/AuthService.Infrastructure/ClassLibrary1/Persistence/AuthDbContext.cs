using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Persistence;

/// <summary>
/// Microsoft Identity + EF Core entegrasyonu.
/// IdentityDbContext: Tüm Identity tablolarını (Users, Roles, Claims…) otomatik yönetir.
/// </summary>
public class AuthDbContext : IdentityDbContext<AppUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tablo adlarını özelleştir (isteğe bağlı)
        builder.Entity<AppUser>(b =>
        {
            b.ToTable("Users");
            b.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            b.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            b.Property(u => u.RefreshToken).HasMaxLength(512);
        });
    }
}
