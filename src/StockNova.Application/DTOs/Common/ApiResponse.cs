namespace StockNova.Application.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }

    public static ApiResponse<T> Fail(string error)
    {
        return new ApiResponse<T> { Success = false, Message = error, Errors = new List<string> { error } };
    }

    public static ApiResponse<T> Fail(string message, List<string> errors)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }

    public static ApiResponse<T> Fail(List<string> errors)
    {
        return new ApiResponse<T> { Success = false, Message = errors.FirstOrDefault() ?? "Error", Errors = errors };
    }
}
