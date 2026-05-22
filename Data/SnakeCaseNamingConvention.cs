using System.Text.RegularExpressions;

namespace InventoryManagement.API.Data;

public static class SnakeCaseNamingConvention
{
    // Konversi PascalCase/camelCase ke snake_case
    // Contoh: AssetCode → asset_code, CreatedAt → created_at
    public static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        // Sisipkan underscore sebelum huruf kapital
        var result = Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2");
        return result.ToLower();
    }
}