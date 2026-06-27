using InventoryManagement.API.DTOs.Department;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Repositories.Interfaces;

public interface IDepartmentRepository
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetDtoByIdAsync(int id);
    Task<Department?> GetByIdWithEmployeesAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task<Department> UpdateAsync(Department department);
    Task DeleteAsync(Department department);
}
