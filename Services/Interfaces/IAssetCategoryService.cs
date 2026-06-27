using InventoryManagement.API.DTOs.AssetCategory;

namespace InventoryManagement.API.Services.Interfaces;

public interface IAssetCategoryService
{
    Task<IEnumerable<AssetCategoryDto>> GetAllAsync();
    Task<AssetCategoryDto?> GetByIdAsync(int id);
    Task<AssetCategoryDto> CreateAsync(CreateAssetCategoryDto dto);
    Task<AssetCategoryDto?> UpdateAsync(int id, UpdateAssetCategoryDto dto);
    Task<bool> DeleteAsync(int id);
}
