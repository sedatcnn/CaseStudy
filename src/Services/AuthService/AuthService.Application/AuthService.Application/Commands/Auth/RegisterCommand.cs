using MediatR;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Commands.Auth;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role = "User") : IRequest<RegisterResult>;

public record RegisterResult(bool Success, string? Message);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterCommandHandler(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Mevcut kullanıcı kontrolü
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return new RegisterResult(false, "Bu e-posta zaten kayıtlı.");

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return new RegisterResult(false, string.Join(", ", result.Errors.Select(e => e.Description)));

        // Rol atama — Policy-Based Authorization desteği için rol oluşturulur
        if (!await _roleManager.RoleExistsAsync(request.Role))
            await _roleManager.CreateAsync(new IdentityRole(request.Role));

        await _userManager.AddToRoleAsync(user, request.Role);

        return new RegisterResult(true, "Kullanıcı başarıyla oluşturuldu.");
    }
}
