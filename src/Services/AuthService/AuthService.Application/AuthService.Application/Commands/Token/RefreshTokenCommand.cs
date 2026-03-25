using MediatR;
using AuthService.Application.Interfaces;
using AuthService.Application.Commands.Auth;   // LoginResult buradan gelir
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Commands.Token;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<LoginResult>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Süresi dolmuş token'dan kullanıcı bilgisi çıkarılır
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
            return new LoginResult(false, null, null, "Geçersiz token.");

        var user = await _userManager.FindByIdAsync(userId);

        // Refresh token geçerliliği kontrol edilir
        if (user is null ||
            user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiry <= DateTime.UtcNow)
            return new LoginResult(false, null, null, "Refresh token geçersiz veya süresi dolmuş.");

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new LoginResult(true, newAccessToken, newRefreshToken, "Token yenilendi.");
    }
}
