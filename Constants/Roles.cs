namespace InventoryManagement.API.Constants;

public static class Roles
{
    public const string Admin    = "Admin";
    public const string Manager  = "Manager";
    public const string Employee = "Employee";

    // Helper untuk validasi role saat register
    public static readonly string[] All = { Admin, Manager, Employee };
}