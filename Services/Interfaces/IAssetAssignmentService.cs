using InventoryManagement.API.DTOs.AssetAssignment;

namespace InventoryManagement.API.Services.Interfaces;

public interface IAssetAssignmentService
{
    Task<IEnumerable<AssetAssignmentDto>> GetAllAsync();
    Task<AssetAssignmentDto?> GetByIdAsync(int id);
    Task<AssetAssignmentDto> AssignAssetAsync(CreateAssetAssignmentDto dto);
    Task<AssetAssignmentDto?> ReturnAssetAsync(int assignmentId, ReturnAssetDto dto);
}