using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.API.DTOs.Asset;

public class UpdateAssetDto
{
    [Required] [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [Required] [Range(0, double.MaxValue)]
    public decimal PurchasePrice { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [Required]
    public int AssetCategoryId { get; set; }
}