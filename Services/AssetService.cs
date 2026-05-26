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
    {
        var assets = await _assetRepository.GetAllAsync();
        return assets.Select(MapToDto);
    }

    public async Task<AssetDto?> GetByIdAsync(int id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        return asset == null ? null : MapToDto(asset);
    }

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

        var created = await _assetRepository.CreateAsync(asset);
        var fullAsset = await _assetRepository.GetByIdAsync(created.Id);
        return MapToDto(fullAsset!);
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
        var updated = await _assetRepository.GetByIdAsync(id);
        return MapToDto(updated!);
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

    private static AssetDto MapToDto(Asset asset) => new()
    {
        Id                = asset.Id,
        AssetCode         = asset.AssetCode,
        Name              = asset.Name,
        Description       = asset.Description,
        SerialNumber      = asset.SerialNumber,
        PurchasePrice     = asset.PurchasePrice,
        PurchaseDate      = asset.PurchaseDate,
        Status            = asset.Status.ToString(),
        AssetCategoryId   = asset.AssetCategoryId,
        AssetCategoryName = asset.AssetCategory?.Name ?? string.Empty
    };

    public async Task<PaginatedResponse<AssetDto>> GetFilteredAsync(AssetFilterDto filter)
    {
        // Validasi page size maksimal 100
        if (filter.PageSize > 100) filter.PageSize = 100;
        if (filter.Page < 1) filter.Page = 1;

        var (items, totalCount) = await _assetRepository.GetFilteredAsync(filter);

        return new PaginatedResponse<AssetDto>
        {
            Items       = items.Select(MapToDto),
            TotalItems  = totalCount,
            TotalPages  = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
            CurrentPage = filter.Page,
            PageSize    = filter.PageSize
        };
    }
}