using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.API.Constants;
using InventoryManagement.API.DTOs.AssetCategory;
using InventoryManagement.API.Helpers;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetCategoriesController : ControllerBase
{
    private readonly IAssetCategoryService _categoryService;

    public AssetCategoriesController(IAssetCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<AssetCategoryDto>>.Ok(categories, "Daftar kategori berhasil diambil."));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category == null
            ? NotFound(ApiResponse.Fail($"Kategori ID {id} tidak ditemukan."))
            : Ok(ApiResponse<AssetCategoryDto>.Ok(category, "Detail kategori berhasil diambil."));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateAssetCategoryDto dto)
    {
        var created = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<AssetCategoryDto>.Ok(created, "Kategori berhasil dibuat."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetCategoryDto dto)
    {
        var updated = await _categoryService.UpdateAsync(id, dto);
        return updated == null
            ? NotFound(ApiResponse.Fail($"Kategori ID {id} tidak ditemukan."))
            : Ok(ApiResponse<AssetCategoryDto>.Ok(updated, "Kategori berhasil diperbarui."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _categoryService.DeleteAsync(id);
            return success
                ? Ok(ApiResponse.Ok("Kategori berhasil dihapus."))
                : NotFound(ApiResponse.Fail($"Kategori ID {id} tidak ditemukan."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }
}
