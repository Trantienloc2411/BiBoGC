namespace Shared.Application.Common;

public class Result<T> 
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public IEnumerable<string> Errors { get; }

    private Result(bool isSuccess, T? value, IEnumerable<string>? errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors ?? [];
    }

    public static Result<T> Success(T value) => new(true, value, [] );
    public static Result<T> Failure(string error) => new (false, default, new[] { error });
    public static Result<T> Failure(IEnumerable<string> errors) => new (false, default, errors);
}

public class Result
{
    public bool IsSuccess { get; set; }
    public IEnumerable<string> Errors { get; }
    private Result(bool isSuccess, IEnumerable<string> errors) => (IsSuccess, Errors) = (isSuccess, errors);
    
    public static Result Success() => new(true, []);
    public static Result Failure(string error) => new(false, [error]);
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);
}