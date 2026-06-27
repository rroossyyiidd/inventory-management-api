namespace InventoryManagement.API.DTOs.Employee;

public class EmployeeFilterDto
{
    public string? Keyword { get; set; }
    public int? DepartmentId { get; set; }
    public string SortBy { get; set; } = "employeeCode";
    public string SortOrder { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
