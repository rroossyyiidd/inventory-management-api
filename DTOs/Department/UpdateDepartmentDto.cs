using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.API.DTOs.Department;

public class UpdateDepartmentDto
{
    [Required(ErrorMessage = "Nama departemen wajib diisi")]
    [StringLength(100, ErrorMessage = "Nama departemen maksimal 100 karakter")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Lokasi maksimal 200 karakter")]
    public string? Location { get; set; }
}
