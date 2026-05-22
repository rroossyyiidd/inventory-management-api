using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.API.DTOs.AssetAssignment;

public class CreateAssetAssignmentDto
{
    [Required]
    public int AssetId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}