namespace AuthService.Application.Common;

/// <summary>
/// Uygulama genelinde kullanılan rol ve politika sabitleri.
/// SRP: Tüm yetki tanımları tek yerde — dağınık string kullanımını önler.
/// OCP: Yeni rol/politika eklemek için sadece bu dosya güncellenir.
/// </summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
}

/// <summary>
/// Policy-Based Authorization tanımları.
/// Rol kombinasyonları veya claim gereksinimleri burada merkezi olarak tutulur.
/// </summary>
public static class AppPolicies
{
    /// <summary>Yalnızca Admin erişebilir.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Admin veya Manager erişebilir (örn: ürün ekleme/güncelleme).</summary>
    public const string ManagerOrAdmin = "ManagerOrAdmin";

    /// <summary>Tüm kayıtlı kullanıcılar erişebilir.</summary>
    public const string AnyUser = "AnyUser";

    /// <summary>
    /// Policy tanımlarını IServiceCollection'a kayıt eder.
    /// DIP: Her servis bu metodu çağırır — tekrar tanımlamaya gerek kalmaz.
    /// </summary>
    public static Microsoft.AspNetCore.Authorization.AuthorizationOptions Register(
        Microsoft.AspNetCore.Authorization.AuthorizationOptions opt)
    {
        // Admin only — log sorgulaması, kullanıcı yönetimi
        opt.AddPolicy(AdminOnly, p => p.RequireRole(AppRoles.Admin));

        // Manager veya Admin — ürün ekleme/güncelleme/silme
        opt.AddPolicy(ManagerOrAdmin, p =>
            p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        // Herhangi bir kayıtlı kullanıcı
        opt.AddPolicy(AnyUser, p =>
            p.RequireRole(AppRoles.User, AppRoles.Manager, AppRoles.Admin));

        return opt;
    }
}
