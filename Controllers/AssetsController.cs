using Microsoft.AspNetCore.Mvc;
using InventoryManagement.API.DTOs.Asset;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var assets = await _assetService.GetAllAsync();
        return Ok(assets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
            return NotFound(new { message = $"Aset dengan ID {id} tidak ditemukan." });
        return Ok(asset);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssetDto dto)
    {
        try
        {
            var created = await _assetService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetDto dto)
    {
        try
        {
            var updated = await _assetService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = $"Aset dengan ID {id} tidak ditemukan." });
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _assetService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = $"Aset dengan ID {id} tidak ditemukan." });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}