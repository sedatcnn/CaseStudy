using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities
{

    /// <summary>
    /// Uygulama kullanıcısı. IdentityUser'dan türetilerek Microsoft Identity ile entegre edildi.
    /// SRP: Yalnızca kimlik bilgilerini ve refresh token'ı yönetir.
    /// </summary>
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
