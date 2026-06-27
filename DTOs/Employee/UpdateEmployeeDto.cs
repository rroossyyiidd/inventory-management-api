using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.API.DTOs.Employee;

public class UpdateEmployeeDto
{
    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    [StringLength(200, ErrorMessage = "Nama lengkap maksimal 200 karakter")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Nomor telepon maksimal 20 karakter")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Departemen wajib dipilih")]
    public int DepartmentId { get; set; }
}
