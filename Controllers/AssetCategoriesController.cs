using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.Models;

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
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.AssetCategories.ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _context.AssetCategories.FindAsync(id);
        return category == null
            ? NotFound(new { message = $"Kategori ID {id} tidak ditemukan." })
            : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssetCategory category)
    {
        category.CreatedAt = DateTime.UtcNow;
        _context.AssetCategories.Add(category);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetCategory updated)
    {
        var category = await _context.AssetCategories.FindAsync(id);
        if (category == null)
            return NotFound(new { message = $"Kategori ID {id} tidak ditemukan." });

        category.Name        = updated.Name;
        category.Description = updated.Description;
        await _context.SaveChangesAsync();
        return Ok(category);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.AssetCategories
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound(new { message = $"Kategori ID {id} tidak ditemukan." });

        if (category.Assets.Any())
            return Conflict(new { message = "Kategori tidak dapat dihapus karena masih ada aset." });

        _context.AssetCategories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}