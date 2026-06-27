using InventoryManagement.API.DTOs.Department;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
        => await _departmentRepository.GetAllAsync();

    public async Task<DepartmentDto?> GetByIdAsync(int id)
        => await _departmentRepository.GetDtoByIdAsync(id);

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = new Department
        {
            Name     = dto.Name.Trim(),
            Location = dto.Location?.Trim()
        };

        var created = await _departmentRepository.CreateAsync(department);

        return new DepartmentDto
        {
            Id        = created.Id,
            Name      = created.Name,
            Location  = created.Location,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
    }

    public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var department = await _departmentRepository.GetByIdWithEmployeesAsync(id);
        if (department == null) return null;

        department.Name     = dto.Name.Trim();
        department.Location = dto.Location?.Trim();

        await _departmentRepository.UpdateAsync(department);
        return await _departmentRepository.GetDtoByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _departmentRepository.GetByIdWithEmployeesAsync(id);
        if (department == null) return false;

        if (department.Employees.Any())
            throw new InvalidOperationException("Departemen tidak dapat dihapus karena masih ada karyawan.");

        await _departmentRepository.DeleteAsync(department);
        return true;
    }
}
