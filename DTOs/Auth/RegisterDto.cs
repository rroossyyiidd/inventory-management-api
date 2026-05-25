using System.ComponentModel.DataAnnotations;
using InventoryManagement.API.Constants;

namespace InventoryManagement.API.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    [MinLength(8, ErrorMessage = "Password minimal 8 karakter")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konfirmasi password wajib diisi")]
    [Compare("Password", ErrorMessage = "Konfirmasi password tidak cocok")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Role opsional — kalau tidak diisi, default ke Employee
    public string Role { get; set; } = Roles.Employee;
}