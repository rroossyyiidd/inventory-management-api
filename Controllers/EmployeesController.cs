using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryManagement.API.Constants;
using InventoryManagement.API.DTOs.Employee;
using InventoryManagement.API.Helpers;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // GET api/employees?keyword=john&departmentId=1&page=1&pageSize=10
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<EmployeeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeFilterDto filter)
    {
        var result = await _employeeService.GetFilteredAsync(filter);
        return Ok(ApiResponse<PaginatedResponse<EmployeeDto>>.Ok(
            result, $"Ditemukan {result.TotalItems} karyawan."));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        return employee == null
            ? NotFound(ApiResponse.Fail($"Karyawan ID {id} tidak ditemukan."))
            : Ok(ApiResponse<EmployeeDto>.Ok(employee, "Detail karyawan berhasil diambil."));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            var created = await _employeeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<EmployeeDto>.Ok(created, "Karyawan berhasil dibuat."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            var updated = await _employeeService.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(ApiResponse.Fail($"Karyawan ID {id} tidak ditemukan."))
                : Ok(ApiResponse<EmployeeDto>.Ok(updated, "Karyawan berhasil diperbarui."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _employeeService.DeleteAsync(id);
        return success
            ? Ok(ApiResponse.Ok("Karyawan berhasil dihapus."))
            : NotFound(ApiResponse.Fail($"Karyawan ID {id} tidak ditemukan."));
    }
}
