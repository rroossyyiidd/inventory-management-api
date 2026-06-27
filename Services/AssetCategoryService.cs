using InventoryManagement.API.DTOs.AssetCategory;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Services;

public class AssetCategoryService : IAssetCategoryService
{
    private readonly IAssetCategoryRepository _categoryRepository;

    public AssetCategoryService(IAssetCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<AssetCategoryDto>> GetAllAsync()
        => await _categoryRepository.GetAllAsync();

    public async Task<AssetCategoryDto?> GetByIdAsync(int id)
        => await _categoryRepository.GetDtoByIdAsync(id);

    public async Task<AssetCategoryDto> CreateAsync(CreateAssetCategoryDto dto)
    {
        var category = new AssetCategory
        {
            Name        = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        };

        var created = await _categoryRepository.CreateAsync(category);

        return new AssetCategoryDto
        {
            Id          = created.Id,
            Name        = created.Name,
            Description = created.Description,
            CreatedAt   = created.CreatedAt,
            UpdatedAt   = created.UpdatedAt
        };
    }

    public async Task<AssetCategoryDto?> UpdateAsync(int id, UpdateAssetCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdWithAssetsAsync(id);
        if (category == null) return null;

        category.Name        = dto.Name.Trim();
        category.Description = dto.Description?.Trim();

        await _categoryRepository.UpdateAsync(category);
        return await _categoryRepository.GetDtoByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdWithAssetsAsync(id);
        if (category == null) return false;

        if (category.Assets.Any())
            throw new InvalidOperationException("Kategori tidak dapat dihapus karena masih ada aset.");

        await _categoryRepository.DeleteAsync(category);
        return true;
    }
}
