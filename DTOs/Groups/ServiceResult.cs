namespace Goal2026API.DTOs.Common;

public sealed class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Code { get; set; } = null!;
    public string Message { get; set; } = null!;
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string message = "OK")
        => new()
        {
            Success = true,
            Code = "OK",
            Message = message,
            Data = data
        };

    public static ServiceResult<T> Fail(string code, string message)
        => new()
        {
            Success = false,
            Code = code,
            Message = message
        };
}