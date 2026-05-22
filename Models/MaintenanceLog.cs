namespace InventoryManagement.API.Models;

public enum MaintenanceType
{
    Preventive,
    Corrective,
    Inspection
}

public class MaintenanceLog
{
    public int Id { get; set; }
    public MaintenanceType Type { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public string? TechnicianName { get; set; }
    public bool IsCompleted { get; set; } = false;

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
}