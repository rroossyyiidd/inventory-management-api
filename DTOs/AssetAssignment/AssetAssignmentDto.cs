namespace InventoryManagement.API.DTOs.AssetAssignment;

public class AssetAssignmentDto
{
    public int Id { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
}