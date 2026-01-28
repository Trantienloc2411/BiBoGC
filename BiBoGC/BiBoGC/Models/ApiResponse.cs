namespace BiBoGC.Models;

/// <summary>
/// Standard API response wrapper
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Response message (usually for errors)
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// List of errors (if any)
    /// </summary>
    public IEnumerable<string>? Errors { get; init; }

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static ApiResponse<T> Error(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

/// <summary>
/// Non-generic API response (for operations without return data)
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Response message
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// List of errors (if any)
    /// </summary>
    public IEnumerable<string>? Errors { get; init; }

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static ApiResponse Error(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}