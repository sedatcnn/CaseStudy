using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Persistence
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products");
                b.HasKey(p => p.Id);
                b.Property(p => p.Name).HasMaxLength(250).IsRequired();
                b.Property(p => p.Description).HasMaxLength(2000);
                b.Property(p => p.Price).HasColumnType("decimal(18,2)");
                b.Property(p => p.Category).HasMaxLength(100);
                b.Property(p => p.CreatedBy).HasMaxLength(450);

                // Sorgular için index
                b.HasIndex(p => p.Category);
                b.HasIndex(p => p.IsActive);
                b.HasIndex(p => p.CreatedAt);
            });
        }
    }

}
