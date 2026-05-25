using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.API.Constants;
using InventoryManagement.API.Data;
using InventoryManagement.API.Models;
using InventoryManagement.API.Helpers;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class AssetCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AssetCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.AssetCategories.ToListAsync();
        return Ok(ApiResponse<List<AssetCategory>>.Ok(categories, "Daftar kategori berhasil diambil."));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _context.AssetCategories.FindAsync(id);
        return category == null
            ? NotFound(ApiResponse.Fail($"Kategori ID {id} tidak ditemukan."))
            : Ok(ApiResponse<AssetCategory>.Ok(category, "Detail kategori berhasil diambil."));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] AssetCategory category)
    {
        category.CreatedAt = DateTime.UtcNow;
        _context.AssetCategories.Add(category);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, ApiResponse<AssetCategory>.Ok(category, "Kategori berhasil dibuat."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetCategory updated)
    {
        var category = await _context.AssetCategories.FindAsync(id);
        if (category == null)
            return NotFound(ApiResponse.Fail($"Kategori ID {id} tidak ditemukan."));

        category.Name        = updated.Name;
        category.Description = updated.Description;
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<AssetCategory>.Ok(category, "Kategori berhasil diperbarui."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.AssetCategories
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound(ApiResponse.Fail($"Kategori ID {id} tidak ditemukan."));

        if (category.Assets.Any())
            return Conflict(ApiResponse.Fail("Kategori tidak dapat dihapus karena masih ada aset."));

        _context.AssetCategories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Kategori berhasil dihapus."));
    }
}