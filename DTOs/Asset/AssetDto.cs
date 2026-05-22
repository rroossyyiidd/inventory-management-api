namespace InventoryManagement.API.DTOs.Asset;

public class AssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int AssetCategoryId { get; set; }
    public string AssetCategoryName { get; set; } = string.Empty;
}