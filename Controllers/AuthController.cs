using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.DTOs.Auth;
using InventoryManagement.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.API.Models;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthController(
        AppDbContext context,
        ITokenService tokenService,
        IConfiguration config)
    {
        _context      = context;
        _tokenService = tokenService;
        _config       = config;
    }

    // POST api/auth/register
    [HttpPost("register")]
    [AllowAnonymous] 
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email.ToLower().Trim());

        if (emailExists)
            return Conflict(new { message = $"Email '{dto.Email}' sudah terdaftar." });

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

        var user = new User
        {
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.ToLower().Trim(),
            PasswordHash = passwordHash,
            Role         = "User",
            IsActive     = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = new AuthResponseDto
        {
            Id        = user.Id,
            FullName  = user.FullName,
            Email     = user.Email,
            Role      = user.Role,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(GetProfile), new { id = user.Id }, response);
    }

    // POST api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // ✅ Cari user berdasarkan email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

        // ✅ Sengaja pesan error dibuat sama untuk email & password
        // agar attacker tidak tahu mana yang salah
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Email atau password salah." });

        // ✅ Verifikasi password dengan BCrypt
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
            return Unauthorized(new { message = "Email atau password salah." });

        // ✅ Update LastLoginAt
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // ✅ Generate JWT token
        var token      = _tokenService.GenerateToken(user);
        var expiresIn  = int.Parse(_config["Jwt:ExpiresInMinutes"]!);

        return Ok(new LoginResponseDto
        {
            Token          = token,
            TokenType      = "Bearer",
            ExpiresInMinutes = expiresIn,
            ExpiresAt      = DateTime.UtcNow.AddMinutes(expiresIn),
            User = new UserInfoDto
            {
                Id       = user.Id,
                FullName = user.FullName,
                Email    = user.Email,
                Role     = user.Role
            }
        });
    }

    // GET api/auth/users/{id}
    [HttpGet("users/{id:int}")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "User tidak ditemukan." });

        return Ok(new AuthResponseDto
        {
            Id        = user.Id,
            FullName  = user.FullName,
            Email     = user.Email,
            Role      = user.Role,
            CreatedAt = user.CreatedAt
        });
    }

     // GET api/auth/me
    // Endpoint untuk cek siapa user yang sedang login
    [HttpGet("me")]
    [Authorize]     // ← wajib login, tanpa token akan dapat 401
    public IActionResult Me()
    {
        // Baca claims dari token via HttpContext.User
        // Tidak perlu query database sama sekali!
        var userId    = User.GetUserId();
        var userEmail = User.GetUserEmail();
        var userName  = User.GetUserName();
        var userRole  = User.GetUserRole();

        return Ok(new
        {
            id    = userId,
            name  = userName,
            email = userEmail,
            role  = userRole,
            isAdmin = User.IsAdmin()
        });
    }
}