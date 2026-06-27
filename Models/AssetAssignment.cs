namespace InventoryManagement.API.Models;

public class AssetAssignment : ISoftDelete
{
    public int Id { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
