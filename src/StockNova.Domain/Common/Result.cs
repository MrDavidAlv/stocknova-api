namespace StockNova.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public List<string> Errors { get; } = new();

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
        Errors.Add(error);
    }

    private Result(List<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
        Error = errors.FirstOrDefault();
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
    public static Result<T> Failure(List<string> errors) => new(errors);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public List<string> Errors { get; } = new();

    private Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
        Errors.Add(error);
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(error);
}
