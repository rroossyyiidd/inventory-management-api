using InventoryManagement.API.DTOs.AssetCategory;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Repositories.Interfaces;

public interface IAssetCategoryRepository
{
    Task<IEnumerable<AssetCategoryDto>> GetAllAsync();
    Task<AssetCategoryDto?> GetDtoByIdAsync(int id);
    Task<AssetCategory?> GetByIdWithAssetsAsync(int id);
    Task<AssetCategory> CreateAsync(AssetCategory category);
    Task<AssetCategory> UpdateAsync(AssetCategory category);
    Task DeleteAsync(AssetCategory category);
}
