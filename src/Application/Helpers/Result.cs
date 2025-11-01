namespace EduCare.Application.Helpers;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public Error Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = Error.None;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Value = default!;
        Error = error;
    }

    public static Result<T> Succeeded(T value) => new Result<T>(value);
    public static Result<T> Failed(Error error) => new Result<T>(error);
}

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    private Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public static Error NotFound(string code, string description)
        => new Error(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description)
        => new Error(code, description, ErrorType.Validation);

    public static Error Failure(string code, string description)
        => new Error(code, description, ErrorType.Failure);

    public static Error Conflict(string code, string description)
        => new Error(code, description, ErrorType.Conflict);
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3
}