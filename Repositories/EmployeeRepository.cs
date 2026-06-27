using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.DTOs.Employee;
using InventoryManagement.API.Models;
using InventoryManagement.API.Repositories.Interfaces;

namespace InventoryManagement.API.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<EmployeeDto> Items, int TotalCount)> GetFilteredAsync(EmployeeFilterDto filter)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.ToLower().Trim();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(keyword) ||
                e.EmployeeCode.ToLower().Contains(keyword) ||
                e.Email.ToLower().Contains(keyword));
        }

        if (filter.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);

        var totalCount = await query.CountAsync();

        var sortedQuery = filter.SortBy.ToLower() switch
        {
            "fullname"   => filter.SortOrder == "desc"
                             ? query.OrderByDescending(e => e.FullName)
                             : query.OrderBy(e => e.FullName),
            "department" => filter.SortOrder == "desc"
                             ? query.OrderByDescending(e => e.Department.Name)
                             : query.OrderBy(e => e.Department.Name),
            _            => query.OrderBy(e => e.EmployeeCode)
        };

        var items = await sortedQuery
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new EmployeeDto
            {
                Id             = e.Id,
                EmployeeCode   = e.EmployeeCode,
                FullName       = e.FullName,
                Email          = e.Email,
                PhoneNumber    = e.PhoneNumber,
                DepartmentId   = e.DepartmentId,
                DepartmentName = e.Department.Name,
                CreatedAt      = e.CreatedAt,
                UpdatedAt      = e.UpdatedAt
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<EmployeeDto?> GetDtoByIdAsync(int id)
    {
        return await _context.Employees
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDto
            {
                Id             = e.Id,
                EmployeeCode   = e.EmployeeCode,
                FullName       = e.FullName,
                Email          = e.Email,
                PhoneNumber    = e.PhoneNumber,
                DepartmentId   = e.DepartmentId,
                DepartmentName = e.Department.Name,
                CreatedAt      = e.CreatedAt,
                UpdatedAt      = e.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> IsCodeExistsAsync(string employeeCode, int? excludeId = null)
    {
        var query = _context.Employees.Where(e => e.EmployeeCode == employeeCode);
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
    {
        var query = _context.Employees.Where(e => e.Email == email);
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task DeleteAsync(Employee employee)
    {
        employee.DeletedAt = DateTime.UtcNow;
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }
}
