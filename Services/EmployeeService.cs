using InventoryManagement.API.DTOs.Employee;
using InventoryManagement.API.Helpers;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;
using InventoryManagement.API.Services.Interfaces;

namespace InventoryManagement.API.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<PaginatedResponse<EmployeeDto>> GetFilteredAsync(EmployeeFilterDto filter)
    {
        if (filter.PageSize > 100) filter.PageSize = 100;
        if (filter.Page < 1) filter.Page = 1;

        var (items, totalCount) = await _employeeRepository.GetFilteredAsync(filter);

        return new PaginatedResponse<EmployeeDto>
        {
            Items       = items,
            TotalItems  = totalCount,
            TotalPages  = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
            CurrentPage = filter.Page,
            PageSize    = filter.PageSize
        };
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
        => await _employeeRepository.GetDtoByIdAsync(id);

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        if (await _employeeRepository.IsCodeExistsAsync(dto.EmployeeCode.ToUpper().Trim()))
            throw new InvalidOperationException($"Kode karyawan '{dto.EmployeeCode}' sudah digunakan.");

        if (await _employeeRepository.IsEmailExistsAsync(dto.Email.ToLower().Trim()))
            throw new InvalidOperationException($"Email '{dto.Email}' sudah terdaftar.");

        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode.ToUpper().Trim(),
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.ToLower().Trim(),
            PhoneNumber  = dto.PhoneNumber?.Trim(),
            DepartmentId = dto.DepartmentId
        };

        await _employeeRepository.CreateAsync(employee);
        return (await _employeeRepository.GetDtoByIdAsync(employee.Id))!;
    }

    public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null) return null;

        if (await _employeeRepository.IsEmailExistsAsync(dto.Email.ToLower().Trim(), excludeId: id))
            throw new InvalidOperationException($"Email '{dto.Email}' sudah digunakan oleh karyawan lain.");

        employee.FullName     = dto.FullName.Trim();
        employee.Email        = dto.Email.ToLower().Trim();
        employee.PhoneNumber  = dto.PhoneNumber?.Trim();
        employee.DepartmentId = dto.DepartmentId;

        await _employeeRepository.UpdateAsync(employee);
        return await _employeeRepository.GetDtoByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null) return false;

        await _employeeRepository.DeleteAsync(employee);
        return true;
    }
}
