namespace InventoryManagement.API.Models;

public class AssetCategory : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
