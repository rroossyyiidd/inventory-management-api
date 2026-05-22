using InventoryManagement.API.DTOs.AssetAssignment;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Services;

public class AssetAssignmentService : IAssetAssignmentService
{
    private readonly IAssetAssignmentRepository _assignmentRepo;
    private readonly IAssetRepository _assetRepo;

    public AssetAssignmentService(
        IAssetAssignmentRepository assignmentRepo,
        IAssetRepository assetRepo)
    {
        _assignmentRepo = assignmentRepo;
        _assetRepo      = assetRepo;
    }

    public async Task<IEnumerable<AssetAssignmentDto>> GetAllAsync()
    {
        var assignments = await _assignmentRepo.GetAllAsync();
        return assignments.Select(MapToDto);
    }

    public async Task<AssetAssignmentDto?> GetByIdAsync(int id)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(id);
        return assignment == null ? null : MapToDto(assignment);
    }

    public async Task<AssetAssignmentDto> AssignAssetAsync(CreateAssetAssignmentDto dto)
    {
        var asset = await _assetRepo.GetByIdAsync(dto.AssetId);
        if (asset == null)
            throw new KeyNotFoundException($"Aset dengan ID {dto.AssetId} tidak ditemukan.");

        if (asset.Status != AssetStatus.Available)
            throw new InvalidOperationException(
                $"Aset '{asset.Name}' tidak dapat ditugaskan. Status saat ini: {asset.Status}.");

        var existingAssignment = await _assignmentRepo.GetActiveAssignmentByAssetIdAsync(dto.AssetId);
        if (existingAssignment != null)
            throw new InvalidOperationException(
                $"Aset ini sudah ditugaskan ke {existingAssignment.Employee.FullName}.");

        var assignment = new AssetAssignment
        {
            AssetId    = dto.AssetId,
            EmployeeId = dto.EmployeeId,
            Notes      = dto.Notes,
            AssignedAt = DateTime.UtcNow,
            IsActive   = true
        };

        asset.Status = AssetStatus.InUse;
        await _assetRepo.UpdateAsync(asset);

        var created = await _assignmentRepo.CreateAsync(assignment);
        var full    = await _assignmentRepo.GetByIdAsync(created.Id);
        return MapToDto(full!);
    }

    public async Task<AssetAssignmentDto?> ReturnAssetAsync(int assignmentId, ReturnAssetDto dto)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(assignmentId);
        if (assignment == null) return null;

        if (!assignment.IsActive)
            throw new InvalidOperationException("Aset ini sudah dikembalikan sebelumnya.");

        assignment.IsActive   = false;
        assignment.ReturnedAt = DateTime.UtcNow;
        if (dto.Notes != null) assignment.Notes = dto.Notes;

        await _assignmentRepo.UpdateAsync(assignment);

        var asset = await _assetRepo.GetByIdAsync(assignment.AssetId);
        if (asset != null)
        {
            asset.Status = AssetStatus.Available;
            await _assetRepo.UpdateAsync(asset);
        }

        var updated = await _assignmentRepo.GetByIdAsync(assignmentId);
        return MapToDto(updated!);
    }

    private static AssetAssignmentDto MapToDto(AssetAssignment aa) => new()
    {
        Id           = aa.Id,
        AssignedAt   = aa.AssignedAt,
        ReturnedAt   = aa.ReturnedAt,
        Notes        = aa.Notes,
        IsActive     = aa.IsActive,
        AssetId      = aa.AssetId,
        AssetCode    = aa.Asset?.AssetCode       ?? string.Empty,
        AssetName    = aa.Asset?.Name            ?? string.Empty,
        EmployeeId   = aa.EmployeeId,
        EmployeeCode = aa.Employee?.EmployeeCode ?? string.Empty,
        EmployeeName = aa.Employee?.FullName     ?? string.Empty
    };
}