using InventoryManagement.API.DTOs.Employee;
using InventoryManagement.API.Helpers;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<(IEnumerable<EmployeeDto> Items, int TotalCount)> GetFilteredAsync(EmployeeFilterDto filter);
    Task<EmployeeDto?> GetDtoByIdAsync(int id);
    Task<Employee?> GetByIdAsync(int id);
    Task<bool> IsCodeExistsAsync(string employeeCode, int? excludeId = null);
    Task<bool> IsEmailExistsAsync(string email, int? excludeId = null);
    Task<Employee> CreateAsync(Employee employee);
    Task<Employee> UpdateAsync(Employee employee);
    Task DeleteAsync(Employee employee);
}
