namespace AuthService.Application.Interfaces;

/// <summary>
/// Token servisi soyutlaması.
/// DIP: Uygulama katmanı, Infrastructure'a değil bu arayüze bağımlıdır.
/// ISP: Yalnızca token işlemleri burada — kullanıcı yönetimi ayrı arayüzde.
/// </summary>
public interface ITokenService
{
    /// <summary>JWT access token üretir.</summary>
    string GenerateAccessToken(string userId, string email, IList<string> roles);

    /// <summary>Kriptografik güçlü refresh token üretir.</summary>
    string GenerateRefreshToken();

    /// <summary>Süresi dolmuş token'dan ClaimsPrincipal çıkarır (refresh akışı için).</summary>
    System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
