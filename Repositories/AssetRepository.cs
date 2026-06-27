using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.DTOs.Asset;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;

namespace InventoryManagement.API.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _context;

    public AssetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetDto>> GetAllAsync()
    {
        return await _context.Assets
            .OrderBy(a => a.AssetCode)
            .Select(a => new AssetDto
            {
                Id                = a.Id,
                AssetCode         = a.AssetCode,
                Name              = a.Name,
                Description       = a.Description,
                SerialNumber      = a.SerialNumber,
                PurchasePrice     = a.PurchasePrice,
                PurchaseDate      = a.PurchaseDate,
                Status            = a.Status.ToString(),
                CreatedAt         = a.CreatedAt,
                UpdatedAt         = a.UpdatedAt,
                AssetCategoryId   = a.AssetCategoryId,
                AssetCategoryName = a.AssetCategory.Name
            })
            .ToListAsync();
    }

    public async Task<AssetDto?> GetPublicDtoByIdAsync(int id)
    {
        return await _context.Assets
            .Where(a => a.Id == id)
            .Select(a => new AssetDto
            {
                Id                = a.Id,
                AssetCode         = a.AssetCode,
                Name              = a.Name,
                Description       = a.Description,
                SerialNumber      = a.SerialNumber,
                PurchasePrice     = a.PurchasePrice,
                PurchaseDate      = a.PurchaseDate,
                Status            = a.Status.ToString(),
                CreatedAt         = a.CreatedAt,
                UpdatedAt         = a.UpdatedAt,
                AssetCategoryId   = a.AssetCategoryId,
                AssetCategoryName = a.AssetCategory.Name
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AssetAdminDto?> GetAdminDtoByIdAsync(int id)
    {
        return await _context.Assets
            .Where(a => a.Id == id)
            .Select(a => new AssetAdminDto
            {
                Id                = a.Id,
                AssetCode         = a.AssetCode,
                Name              = a.Name,
                Description       = a.Description,
                SerialNumber      = a.SerialNumber,
                PurchasePrice     = a.PurchasePrice,
                PurchaseDate      = a.PurchaseDate,
                Status            = a.Status.ToString(),
                CreatedAt         = a.CreatedAt,
                UpdatedAt         = a.UpdatedAt,
                AssetCategoryId   = a.AssetCategoryId,
                AssetCategoryName = a.AssetCategory.Name,
                Assignments = a.AssetAssignments.Select(aa => new AssignmentSummaryDto
                {
                    Id           = aa.Id,
                    EmployeeId   = aa.EmployeeId,
                    EmployeeName = aa.Employee.FullName,
                    EmployeeCode = aa.Employee.EmployeeCode,
                    AssignedAt   = aa.AssignedAt,
                    ReturnedAt   = aa.ReturnedAt,
                    IsActive     = aa.IsActive,
                    Notes        = aa.Notes
                }),
                MaintenanceLogs = a.MaintenanceLogs.Select(ml => new MaintenanceSummaryDto
                {
                    Id             = ml.Id,
                    Type           = ml.Type.ToString(),
                    ScheduledAt    = ml.ScheduledAt,
                    CompletedAt    = ml.CompletedAt,
                    Description    = ml.Description,
                    Cost           = ml.Cost,
                    TechnicianName = ml.TechnicianName,
                    IsCompleted    = ml.IsCompleted
                })
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.AssetCategory)
            .Include(a => a.AssetAssignments)
            .Include(a => a.MaintenanceLogs)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> IsCodeExistsAsync(string assetCode, int? excludeId = null)
    {
        var query = _context.Assets.Where(a => a.AssetCode == assetCode);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<Asset> CreateAsync(Asset asset)
    {
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        // Load category name without re-fetching the full asset (H2 fix)
        await _context.Entry(asset).Reference(a => a.AssetCategory).LoadAsync();
        return asset;
    }

    public async Task<Asset> UpdateAsync(Asset asset)
    {
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task DeleteAsync(Asset asset)
    {
        var now = DateTime.UtcNow;
        asset.DeletedAt = now;

        // Cascade soft-delete so orphan check isn't needed across queries
        foreach (var assignment in asset.AssetAssignments.Where(a => a.DeletedAt == null))
            assignment.DeletedAt = now;

        foreach (var log in asset.MaintenanceLogs.Where(l => l.DeletedAt == null))
            log.DeletedAt = now;

        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Asset>> GetAllIncludingDeletedAsync()
    {
        return await _context.Assets
            .IgnoreQueryFilters()
            .Include(a => a.AssetCategory)
            .ToListAsync();
    }

    public async Task<(IEnumerable<AssetDto> Items, int TotalCount)> GetFilteredAsync(AssetFilterDto filter)
    {
        var query = _context.Assets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.ToLower().Trim();
            query = query.Where(a =>
                a.Name.ToLower().Contains(keyword) ||
                a.AssetCode.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (Enum.TryParse<AssetStatus>(filter.Status, true, out var status))
                query = query.Where(a => a.Status == status);
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(a => a.AssetCategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(a => a.PurchasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(a => a.PurchasePrice <= filter.MaxPrice.Value);

        if (filter.PurchaseDateFrom.HasValue)
            query = query.Where(a => a.PurchaseDate >= filter.PurchaseDateFrom.Value);

        if (filter.PurchaseDateTo.HasValue)
            query = query.Where(a => a.PurchaseDate <= filter.PurchaseDateTo.Value);

        var totalCount = await query.CountAsync();

        var sortedQuery = filter.SortBy.ToLower() switch
        {
            "name"          => filter.SortOrder == "desc"
                                ? query.OrderByDescending(a => a.Name)
                                : query.OrderBy(a => a.Name),
            "purchaseprice" => filter.SortOrder == "desc"
                                ? query.OrderByDescending(a => a.PurchasePrice)
                                : query.OrderBy(a => a.PurchasePrice),
            "purchasedate"  => filter.SortOrder == "desc"
                                ? query.OrderByDescending(a => a.PurchaseDate)
                                : query.OrderBy(a => a.PurchaseDate),
            "status"        => filter.SortOrder == "desc"
                                ? query.OrderByDescending(a => a.Status)
                                : query.OrderBy(a => a.Status),
            _               => query.OrderBy(a => a.AssetCode)
        };

        var items = await sortedQuery
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AssetDto
            {
                Id                = a.Id,
                AssetCode         = a.AssetCode,
                Name              = a.Name,
                Description       = a.Description,
                SerialNumber      = a.SerialNumber,
                PurchasePrice     = a.PurchasePrice,
                PurchaseDate      = a.PurchaseDate,
                Status            = a.Status.ToString(),
                CreatedAt         = a.CreatedAt,
                UpdatedAt         = a.UpdatedAt,
                AssetCategoryId   = a.AssetCategoryId,
                AssetCategoryName = a.AssetCategory.Name
            })
            .ToListAsync();

        return (items, totalCount);
    }
}
