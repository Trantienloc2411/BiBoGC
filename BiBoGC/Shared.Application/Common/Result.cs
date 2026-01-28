namespace Shared.Application.Common;

public class Result<T>
{
    private Result(bool isSuccess, T? value, IEnumerable<string>? errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors ?? [];
    }

    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public IEnumerable<string> Errors { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, []);
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, new[] { error });
    }

    public static Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default, errors);
    }
}

public class Result
{
    private Result(bool isSuccess, IEnumerable<string> errors)
    {
        (IsSuccess, Errors) = (isSuccess, errors);
    }

    public bool IsSuccess { get; set; }
    public IEnumerable<string> Errors { get; }

    public static Result Success()
    {
        return new Result(true, []);
    }

    public static Result Failure(string error)
    {
        return new Result(false, [error]);
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }
}