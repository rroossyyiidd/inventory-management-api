using InventoryManagement.API.DTOs.Asset;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Repositories.Interfaces;

public interface IAssetRepository
{
    Task<IEnumerable<AssetDto>> GetAllAsync();
    Task<(IEnumerable<AssetDto> Items, int TotalCount)> GetFilteredAsync(AssetFilterDto filter);
    Task<AssetDto?> GetPublicDtoByIdAsync(int id);
    Task<AssetAdminDto?> GetAdminDtoByIdAsync(int id);

    // Entity methods for mutations
    Task<Asset?> GetByIdAsync(int id);
    Task<bool> IsCodeExistsAsync(string assetCode, int? excludeId = null);
    Task<Asset> CreateAsync(Asset asset);
    Task<Asset> UpdateAsync(Asset asset);
    Task DeleteAsync(Asset asset);
}
