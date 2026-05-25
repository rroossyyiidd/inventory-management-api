using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.API.Constants;
using InventoryManagement.API.DTOs.AssetAssignment;
using InventoryManagement.API.Services.Interfaces;
using InventoryManagement.API.Helpers;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class AssetAssignmentsController : ControllerBase
{
    private readonly IAssetAssignmentService _assignmentService;

    public AssetAssignmentsController(IAssetAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> GetAll()
    {
        var assignments = await _assignmentService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<AssetAssignmentDto>>.Ok(assignments, "Daftar assignment berhasil diambil."));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> GetById(int id)
    {
        var assignment = await _assignmentService.GetByIdAsync(id);
        if (assignment == null)
            return NotFound(ApiResponse.Fail($"Assignment dengan ID {id} tidak ditemukan."));
        return Ok(ApiResponse<AssetAssignmentDto>.Ok(assignment, "Detail assignment berhasil diambil."));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> AssignAsset([FromBody] CreateAssetAssignmentDto dto)
    {
        try
        {
            var created = await _assignmentService.AssignAssetAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<AssetAssignmentDto>.Ok(created, "Aset berhasil ditugaskan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}/return")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> ReturnAsset(int id, [FromBody] ReturnAssetDto dto)
    {
        try
        {
            var updated = await _assignmentService.ReturnAssetAsync(id, dto);
            if (updated == null)
                return NotFound(ApiResponse.Fail($"Assignment dengan ID {id} tidak ditemukan."));
            return Ok(ApiResponse<AssetAssignmentDto>.Ok(updated, "Aset berhasil dikembalikan."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}