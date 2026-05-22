using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;

namespace InventoryManagement.API.Repositories;

public class AssetAssignmentRepository : IAssetAssignmentRepository
{
    private readonly AppDbContext _context;

    public AssetAssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetAssignment>> GetAllAsync()
    {
        return await _context.AssetAssignments
            .Include(aa => aa.Asset)
            .Include(aa => aa.Employee)
            .OrderByDescending(aa => aa.AssignedAt)
            .ToListAsync();
    }

    public async Task<AssetAssignment?> GetByIdAsync(int id)
    {
        return await _context.AssetAssignments
            .Include(aa => aa.Asset)
                .ThenInclude(a => a.AssetCategory)
            .Include(aa => aa.Employee)
                .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(aa => aa.Id == id);
    }

    public async Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId)
    {
        return await _context.AssetAssignments
            .Include(aa => aa.Employee)
            .FirstOrDefaultAsync(aa => aa.AssetId == assetId && aa.IsActive);
    }

    public async Task<AssetAssignment> CreateAsync(AssetAssignment assignment)
    {
        _context.AssetAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<AssetAssignment> UpdateAsync(AssetAssignment assignment)
    {
        _context.AssetAssignments.Update(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }
}