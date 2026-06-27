namespace InventoryManagement.API.DTOs.Asset;

public class AssignmentSummaryDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class MaintenanceSummaryDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public string? TechnicianName { get; set; }
    public bool IsCompleted { get; set; }
}

public class AssetAdminDto : AssetDto
{
    public IEnumerable<AssignmentSummaryDto> Assignments { get; set; } = new List<AssignmentSummaryDto>();
    public IEnumerable<MaintenanceSummaryDto> MaintenanceLogs { get; set; } = new List<MaintenanceSummaryDto>();
}
