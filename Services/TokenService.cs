using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using InventoryManagement.API.Models;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        // Ambil konfigurasi JWT dari appsettings.json
        var key       = _config["Jwt:Key"]!;
        var issuer    = _config["Jwt:Issuer"]!;
        var audience  = _config["Jwt:Audience"]!;
        var expiresIn = int.Parse(_config["Jwt:ExpiresInMinutes"]!);

        // Claims = informasi yang disimpan di dalam token
        // Siapapun yang punya token bisa baca claims ini (tapi tidak bisa palsu)
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.FullName),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Role,           user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // ID unik per token
            new(JwtRegisteredClaimNames.Iat,                              // Waktu token dibuat
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        // Buat signing key dari secret key di appsettings
        var signingKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Buat token
        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: credentials
        );

        // Serialisasi token ke string (format: xxxxx.yyyyy.zzzzz)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}