namespace InventoryManagement.API.Models;

public class Department : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}