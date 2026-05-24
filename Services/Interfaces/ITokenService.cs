using InventoryManagement.API.Models;

namespace InventoryManagement.API.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}