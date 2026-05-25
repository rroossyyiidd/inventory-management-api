namespace InventoryManagement.API.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    // Static factory methods — agar mudah dipakai di controller
    public static ApiResponse<T> Ok(T data, string message = "Berhasil.")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message, Data = default };
}

// Versi tanpa generic — untuk response yang tidak punya data (DELETE)
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static ApiResponse Ok(string message = "Berhasil.")
        => new() { Success = true, Message = message, Data = null };

    public static ApiResponse Fail(string message)
        => new() { Success = false, Message = message, Data = null };
}