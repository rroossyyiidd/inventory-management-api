using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.DTOs.Auth;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // POST api/auth/register
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // ✅ Validasi email sudah terdaftar
        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email.ToLower().Trim());

        if (emailExists)
            return Conflict(new { message = $"Email '{dto.Email}' sudah terdaftar." });

        // ✅ Hash password menggunakan BCrypt
        // workFactor = 12 → makin tinggi makin aman tapi makin lambat
        // 12 adalah nilai yang direkomendasikan untuk production
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

    // GET api/auth/users/{id}
    // Endpoint helper untuk CreatedAtAction di atas
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
}