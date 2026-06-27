using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Constants;
using InventoryManagement.API.Data;
using InventoryManagement.API.DTOs.Auth;
using InventoryManagement.API.Models;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, ITokenService tokenService, IConfiguration config)
    {
        _context      = context;
        _tokenService = tokenService;
        _config       = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, bool isCallerAuthenticated, bool isCallerAdmin)
    {
        if (!Roles.All.Contains(dto.Role))
            throw new ArgumentException($"Role tidak valid. Role yang tersedia: {string.Join(", ", Roles.All)}");

        if (dto.Role != Roles.Employee && !isCallerAuthenticated)
            throw new ArgumentException("Hanya Admin yang dapat mendaftarkan user dengan role Admin atau Manager.");

        if (dto.Role != Roles.Employee && isCallerAuthenticated && !isCallerAdmin)
            throw new UnauthorizedAccessException();

        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email.ToLower().Trim());

        if (emailExists)
            throw new InvalidOperationException($"Email '{dto.Email}' sudah terdaftar.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

        var user = new User
        {
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.ToLower().Trim(),
            PasswordHash = passwordHash,
            Role         = dto.Role,
            IsActive     = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Id        = user.Id,
            FullName  = user.FullName,
            Email     = user.Email,
            Role      = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Email atau password salah.");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Email atau password salah.");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token     = _tokenService.GenerateToken(user);
        var expiresIn = int.Parse(_config["Jwt:ExpiresInMinutes"]!);

        return new LoginResponseDto
        {
            Token            = token,
            TokenType        = "Bearer",
            ExpiresInMinutes = expiresIn,
            ExpiresAt        = DateTime.UtcNow.AddMinutes(expiresIn),
            User = new UserInfoDto
            {
                Id       = user.Id,
                FullName = user.FullName,
                Email    = user.Email,
                Role     = user.Role
            }
        };
    }

    public async Task<AuthResponseDto?> GetProfileAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        return new AuthResponseDto
        {
            Id        = user.Id,
            FullName  = user.FullName,
            Email     = user.Email,
            Role      = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
