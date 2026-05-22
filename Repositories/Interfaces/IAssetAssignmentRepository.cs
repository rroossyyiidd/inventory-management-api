using InventoryManagement.API.Models;

namespace InventoryManagement.API.Repositories.Interfaces;

public interface IAssetAssignmentRepository
{
    Task<IEnumerable<AssetAssignment>> GetAllAsync();
    Task<AssetAssignment?> GetByIdAsync(int id);
    Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId);
    Task<AssetAssignment> CreateAsync(AssetAssignment assignment);
    Task<AssetAssignment> UpdateAsync(AssetAssignment assignment);
}