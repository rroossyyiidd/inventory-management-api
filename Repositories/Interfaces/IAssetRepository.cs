using InventoryManagement.API.Models;

namespace InventoryManagement.API.Repositories.Interfaces;

public interface IAssetRepository
{
    Task<IEnumerable<Asset>> GetAllAsync();
    Task<Asset?> GetByIdAsync(int id);
    Task<bool> IsCodeExistsAsync(string assetCode, int? excludeId = null);
    Task<Asset> CreateAsync(Asset asset);
    Task<Asset> UpdateAsync(Asset asset);
    Task DeleteAsync(Asset asset);
}