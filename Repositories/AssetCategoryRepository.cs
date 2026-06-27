using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.DTOs.AssetCategory;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;

namespace InventoryManagement.API.Repositories;

public class AssetCategoryRepository : IAssetCategoryRepository
{
    private readonly AppDbContext _context;

    public AssetCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetCategoryDto>> GetAllAsync()
    {
        return await _context.AssetCategories
            .OrderBy(c => c.Name)
            .Select(c => new AssetCategoryDto
            {
                Id          = c.Id,
                Name        = c.Name,
                Description = c.Description,
                CreatedAt   = c.CreatedAt,
                UpdatedAt   = c.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<AssetCategoryDto?> GetDtoByIdAsync(int id)
    {
        return await _context.AssetCategories
            .Where(c => c.Id == id)
            .Select(c => new AssetCategoryDto
            {
                Id          = c.Id,
                Name        = c.Name,
                Description = c.Description,
                CreatedAt   = c.CreatedAt,
                UpdatedAt   = c.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AssetCategory?> GetByIdWithAssetsAsync(int id)
    {
        return await _context.AssetCategories
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<AssetCategory> CreateAsync(AssetCategory category)
    {
        _context.AssetCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<AssetCategory> UpdateAsync(AssetCategory category)
    {
        _context.AssetCategories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteAsync(AssetCategory category)
    {
        _context.AssetCategories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
