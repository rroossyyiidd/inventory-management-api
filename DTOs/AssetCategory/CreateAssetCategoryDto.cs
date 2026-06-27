using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.API.DTOs.AssetCategory;

public class CreateAssetCategoryDto
{
    [Required(ErrorMessage = "Nama kategori wajib diisi")]
    [StringLength(100, ErrorMessage = "Nama kategori maksimal 100 karakter")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Deskripsi maksimal 500 karakter")]
    public string? Description { get; set; }
}
