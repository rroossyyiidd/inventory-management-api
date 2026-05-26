namespace InventoryManagement.API.DTOs.Asset;

public class AssetFilterDto
{
    // Filter by keyword nama atau asset code
    public string? Keyword { get; set; }

    // Filter by status (Available, InUse, UnderMaintenance, Disposed)
    public string? Status { get; set; }

    // Filter by kategori
    public int? CategoryId { get; set; }

    // Filter by rentang harga
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Filter by rentang tanggal pembelian
    public DateTime? PurchaseDateFrom { get; set; }
    public DateTime? PurchaseDateTo { get; set; }

    // Sorting
    public string SortBy { get; set; } = "assetCode";   // default sort by assetCode
    public string SortOrder { get; set; } = "asc";       // asc atau desc

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}