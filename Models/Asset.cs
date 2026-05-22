namespace InventoryManagement.API.Models;

public enum AssetStatus
{
    Available,
    InUse,
    UnderMaintenance,
    Disposed
}

public class Asset : IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Available;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public int AssetCategoryId { get; set; }
    public AssetCategory AssetCategory { get; set; } = null!;

    public ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();
    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
}