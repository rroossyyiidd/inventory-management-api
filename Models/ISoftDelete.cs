namespace InventoryManagement.API.Models;

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }
    bool IsDeleted => DeletedAt.HasValue;
}