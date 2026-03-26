namespace ProductService.Domain.Entities;

/// <summary>
/// Ürün aggregate root'u.
/// Domain katmanı hiçbir dış bağımlılık içermez — Onion mimarinin çekirdeği.
/// </summary>
public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // EF Core için parametresiz constructor
    protected Product() { }

    /// <summary>
    /// Factory method — nesneyi her zaman geçerli bir durumda oluşturur.
    /// Doğrudan new Product { ... } kullanımını engeller.
    /// </summary>
    public static Product Create(string name, string description,
        decimal price, int stock, string category, string createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (price < 0) throw new ArgumentException("Fiyat negatif olamaz.");
        if (stock < 0) throw new ArgumentException("Stok negatif olamaz.");

        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock,
            Category = category,
            CreatedBy = createdBy
        };
    }

    public void Update(string name, string description, decimal price, int stock, string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (price < 0) throw new ArgumentException("Fiyat negatif olamaz.");

        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}
