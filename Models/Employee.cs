namespace InventoryManagement.API.Models;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();
}