using Microsoft.AspNetCore.Mvc;
using InventoryManagement.API.DTOs.Asset;
using InventoryManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.API.Constants;
using InventoryManagement.API.Helpers;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetAll()
    {
        var assets = await _assetService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<AssetDto>>.Ok(assets, "Daftar aset berhasil diambil."));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetById(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
            return NotFound(ApiResponse.Fail($"Aset dengan ID {id} tidak ditemukan."));
        return Ok(ApiResponse<AssetDto>.Ok(asset, "Detail aset berhasil diambil."));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateAssetDto dto)
    {
        try
        {
            var created = await _assetService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<AssetDto>.Ok(created, "Aset berhasil dibuat."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetDto dto)
    {
        try
        {
            var updated = await _assetService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(ApiResponse.Fail($"Aset dengan ID {id} tidak ditemukan."));
            return Ok(ApiResponse<AssetDto>.Ok(updated, "Aset berhasil diperbarui."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)] 
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _assetService.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponse.Fail($"Aset dengan ID {id} tidak ditemukan."));
            return Ok(ApiResponse.Ok("Aset berhasil dihapus."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    // GET api/assets/filter?keyword=laptop&status=Available&minPrice=1000000&page=1&pageSize=10
    [HttpGet("filter")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AssetDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFiltered([FromQuery] AssetFilterDto filter)
    {
        var result = await _assetService.GetFilteredAsync(filter);
        return Ok(ApiResponse<PaginatedResponse<AssetDto>>.Ok(
            result, $"Ditemukan {result.TotalItems} aset."));
    }
}