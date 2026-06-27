using InventoryManagement.API.DTOs.Employee;
using InventoryManagement.API.Helpers;

namespace InventoryManagement.API.Services.Interfaces;

public interface IEmployeeService
{
    Task<PaginatedResponse<EmployeeDto>> GetFilteredAsync(EmployeeFilterDto filter);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
    Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<bool> DeleteAsync(int id);
}
