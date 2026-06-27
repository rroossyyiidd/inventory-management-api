using InventoryManagement.API.DTOs.Auth;

namespace InventoryManagement.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, bool isCallerAuthenticated, bool isCallerAdmin);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto?> GetProfileAsync(int id);
}
