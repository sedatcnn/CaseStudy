using MediatR;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Commands.Auth;

// ── COMMAND ──────────────────────────────────────────────────────────────────
/// <summary>
/// Kullanıcı giriş komutu.
/// CQRS: Yan etkisi olan işlem → Command. Sorgulardan tamamen ayrıdır.
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(bool Success, string? AccessToken, string? RefreshToken, string? Message);

// ── HANDLER ──────────────────────────────────────────────────────────────────
/// <summary>
/// SRP: Yalnızca login akışını yönetir.
/// OCP: Yeni auth stratejisi eklemek için bu class değişmez, yeni handler eklenir.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı varlığını ve aktivitesini kontrol et
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return new LoginResult(false, null, null, "Geçersiz kimlik bilgileri.");

        // 2. Şifre doğrulama
        var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
            return new LoginResult(false, null, null, "Geçersiz kimlik bilgileri.");

        // 3. Roller alınır (Role-Based Authorization için)
        var roles = await _userManager.GetRolesAsync(user);

        // 4. Token'lar üretilir
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // 5. Refresh token veritabanında saklanır
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new LoginResult(true, accessToken, refreshToken, "Giriş başarılı.");
    }
}
