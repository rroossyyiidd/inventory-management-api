using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.API.DTOs.AssetAssignment;

public class ReturnAssetDto
{
    [StringLength(500)]
    public string? Notes { get; set; }
}