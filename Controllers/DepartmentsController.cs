using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.API.Constants;
using InventoryManagement.API.DTOs.Department;
using InventoryManagement.API.Helpers;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentDto>>.Ok(departments, "Daftar departemen berhasil diambil."));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        return department == null
            ? NotFound(ApiResponse.Fail($"Departemen ID {id} tidak ditemukan."))
            : Ok(ApiResponse<DepartmentDto>.Ok(department, "Detail departemen berhasil diambil."));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var created = await _departmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<DepartmentDto>.Ok(created, "Departemen berhasil dibuat."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var updated = await _departmentService.UpdateAsync(id, dto);
        return updated == null
            ? NotFound(ApiResponse.Fail($"Departemen ID {id} tidak ditemukan."))
            : Ok(ApiResponse<DepartmentDto>.Ok(updated, "Departemen berhasil diperbarui."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _departmentService.DeleteAsync(id);
            return success
                ? Ok(ApiResponse.Ok("Departemen berhasil dihapus."))
                : NotFound(ApiResponse.Fail($"Departemen ID {id} tidak ditemukan."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }
}
