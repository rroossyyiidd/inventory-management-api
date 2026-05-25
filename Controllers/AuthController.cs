using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.DTOs.Auth;
using InventoryManagement.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.API.Models;
using InventoryManagement.API.Services.Interfaces;
using InventoryManagement.API.Constants;

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // ✅ Validasi role harus salah satu dari yang terdaftar
        if (!Roles.All.Contains(dto.Role))
            return BadRequest(new
            {
                message = $"Role tidak valid. Role yang tersedia: {string.Join(", ", Roles.All)}"
            });

        // ✅ Hanya Admin yang boleh membuat user dengan role Admin atau Manager
        // Kalau belum login (register pertama kali), hanya bisa buat role Employee
        if (dto.Role != Roles.Employee && !User.Identity!.IsAuthenticated)
            return BadRequest(new
            {
                message = "Hanya Admin yang dapat mendaftarkan user dengan role Admin atau Manager."
            });

        if (dto.Role != Roles.Employee && User.Identity!.IsAuthenticated && !User.IsAdmin())
            return Forbid();

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
            Role         = dto.Role,
            IsActive     = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProfile), new { id = user.Id }, new AuthResponseDto
        {
            Id        = user.Id,
            FullName  = user.FullName,
            Email     = user.Email,
            Role      = user.Role,
            CreatedAt = user.CreatedAt
        });
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
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        return Ok(new
        {
            id      = User.GetUserId(),
            name    = User.GetUserName(),
            email   = User.GetUserEmail(),
            role    = User.GetUserRole(),
            isAdmin = User.IsAdmin()
        });
    }
}