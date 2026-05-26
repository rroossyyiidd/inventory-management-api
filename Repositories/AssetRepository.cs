using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;
using InventoryManagement.API.DTOs.Asset;

namespace InventoryManagement.API.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _context;

    public AssetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await _context.Assets
            .Include(a => a.AssetCategory)
            .OrderBy(a => a.AssetCode)
            .ToListAsync();
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.AssetCategory)
            .Include(a => a.AssetAssignments)
                .ThenInclude(aa => aa.Employee)
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
        asset.DeletedAt = DateTime.UtcNow;
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    // Di Repository, tambahkan method ini kalau dibutuhkan
    public async Task<IEnumerable<Asset>> GetAllIncludingDeletedAsync()
    {
        return await _context.Assets
            .IgnoreQueryFilters()   // ← bypass global query filter
            .Include(a => a.AssetCategory)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Asset> Items, int TotalCount)> GetFilteredAsync(AssetFilterDto filter)
    {
        // Mulai dari semua asset, belum dieksekusi ke DB
        var query = _context.Assets
            .Include(a => a.AssetCategory)
            .AsQueryable();

        // ✅ Filter 1 — Keyword (cari di nama atau asset code)
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.ToLower().Trim();
            query = query.Where(a =>
                a.Name.ToLower().Contains(keyword) ||
                a.AssetCode.ToLower().Contains(keyword));
        }

        // ✅ Filter 2 — Status
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (Enum.TryParse<AssetStatus>(filter.Status, true, out var status))
                query = query.Where(a => a.Status == status);
        }

        // ✅ Filter 3 — Kategori
        if (filter.CategoryId.HasValue)
            query = query.Where(a => a.AssetCategoryId == filter.CategoryId.Value);

        // ✅ Filter 4 — Rentang harga
        if (filter.MinPrice.HasValue)
            query = query.Where(a => a.PurchasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(a => a.PurchasePrice <= filter.MaxPrice.Value);

        // ✅ Filter 5 — Rentang tanggal pembelian
        if (filter.PurchaseDateFrom.HasValue)
            query = query.Where(a => a.PurchaseDate >= filter.PurchaseDateFrom.Value);

        if (filter.PurchaseDateTo.HasValue)
            query = query.Where(a => a.PurchaseDate <= filter.PurchaseDateTo.Value);

        // Hitung total sebelum pagination
        // Query ke DB baru terjadi di sini untuk hitung total
        var totalCount = await query.CountAsync();

        // ✅ Sorting
        query = filter.SortBy.ToLower() switch
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
            _               => query.OrderBy(a => a.AssetCode)  // default
        };

        // ✅ Pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}