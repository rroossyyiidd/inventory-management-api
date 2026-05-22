using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
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
        asset.UpdatedAt = DateTime.UtcNow;
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task DeleteAsync(Asset asset)
    {
        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
    }
}