using Microsoft.AspNetCore.Mvc;
using InventoryManagement.API.DTOs.AssetAssignment;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetAssignmentsController : ControllerBase
{
    private readonly IAssetAssignmentService _assignmentService;

    public AssetAssignmentsController(IAssetAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var assignments = await _assignmentService.GetAllAsync();
        return Ok(assignments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var assignment = await _assignmentService.GetByIdAsync(id);
        if (assignment == null)
            return NotFound(new { message = $"Assignment dengan ID {id} tidak ditemukan." });
        return Ok(assignment);
    }

    [HttpPost]
    public async Task<IActionResult> AssignAsset([FromBody] CreateAssetAssignmentDto dto)
    {
        try
        {
            var created = await _assignmentService.AssignAssetAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/return")]
    public async Task<IActionResult> ReturnAsset(int id, [FromBody] ReturnAssetDto dto)
    {
        try
        {
            var updated = await _assignmentService.ReturnAssetAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = $"Assignment dengan ID {id} tidak ditemukan." });
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}