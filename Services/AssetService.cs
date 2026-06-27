using InventoryManagement.API.DTOs.Asset;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;
using InventoryManagement.API.Services.Interfaces;
using InventoryManagement.API.Helpers;

namespace InventoryManagement.API.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;

    public AssetService(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<IEnumerable<AssetDto>> GetAllAsync()
        => await _assetRepository.GetAllAsync();

    public async Task<AssetDto?> GetByIdAsync(int id)
        => await _assetRepository.GetPublicDtoByIdAsync(id);

    public async Task<AssetAdminDto?> GetByIdAdminAsync(int id)
        => await _assetRepository.GetAdminDtoByIdAsync(id);

    public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
    {
        bool codeExists = await _assetRepository.IsCodeExistsAsync(dto.AssetCode);
        if (codeExists)
            throw new InvalidOperationException($"Asset code '{dto.AssetCode}' sudah digunakan.");

        var asset = new Asset
        {
            AssetCode       = dto.AssetCode.ToUpper().Trim(),
            Name            = dto.Name,
            Description     = dto.Description,
            SerialNumber    = dto.SerialNumber,
            PurchasePrice   = dto.PurchasePrice,
            PurchaseDate    = dto.PurchaseDate,
            AssetCategoryId = dto.AssetCategoryId,
            Status          = AssetStatus.Available
        };

        // CreateAsync loads AssetCategory ref after INSERT — no second full query needed
        var created = await _assetRepository.CreateAsync(asset);

        return new AssetDto
        {
            Id                = created.Id,
            AssetCode         = created.AssetCode,
            Name              = created.Name,
            Description       = created.Description,
            SerialNumber      = created.SerialNumber,
            PurchasePrice     = created.PurchasePrice,
            PurchaseDate      = created.PurchaseDate,
            Status            = created.Status.ToString(),
            CreatedAt         = created.CreatedAt,
            UpdatedAt         = created.UpdatedAt,
            AssetCategoryId   = created.AssetCategoryId,
            AssetCategoryName = created.AssetCategory?.Name ?? string.Empty
        };
    }

    public async Task<AssetDto?> UpdateAsync(int id, UpdateAssetDto dto)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null) return null;

        if (asset.Status is AssetStatus.InUse or AssetStatus.UnderMaintenance
            && asset.AssetCategoryId != dto.AssetCategoryId)
        {
            throw new InvalidOperationException(
                "Kategori aset tidak dapat diubah saat aset sedang digunakan atau dalam pemeliharaan.");
        }

        asset.Name            = dto.Name;
        asset.Description     = dto.Description;
        asset.SerialNumber    = dto.SerialNumber;
        asset.PurchasePrice   = dto.PurchasePrice;
        asset.PurchaseDate    = dto.PurchaseDate;
        asset.AssetCategoryId = dto.AssetCategoryId;

        await _assetRepository.UpdateAsync(asset);
        return await _assetRepository.GetPublicDtoByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null) return false;

        if (asset.Status == AssetStatus.InUse)
            throw new InvalidOperationException("Aset yang sedang digunakan tidak dapat dihapus.");

        await _assetRepository.DeleteAsync(asset);
        return true;
    }

    public async Task<PaginatedResponse<AssetDto>> GetFilteredAsync(AssetFilterDto filter)
    {
        if (filter.PageSize > 100) filter.PageSize = 100;
        if (filter.Page < 1) filter.Page = 1;

        var (items, totalCount) = await _assetRepository.GetFilteredAsync(filter);

        return new PaginatedResponse<AssetDto>
        {
            Items       = items,
            TotalItems  = totalCount,
            TotalPages  = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
            CurrentPage = filter.Page,
            PageSize    = filter.PageSize
        };
    }
}
