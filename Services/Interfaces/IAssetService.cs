using InventoryManagement.API.DTOs.Asset;

namespace InventoryManagement.API.Services.Interfaces;

public interface IAssetService
{
    Task<IEnumerable<AssetDto>> GetAllAsync();
    Task<AssetDto?> GetByIdAsync(int id);
    Task<AssetDto> CreateAsync(CreateAssetDto dto);
    Task<AssetDto?> UpdateAsync(int id, UpdateAssetDto dto);
    Task<bool> DeleteAsync(int id);
}