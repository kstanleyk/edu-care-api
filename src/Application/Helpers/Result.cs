namespace EduCare.Application.Helpers;

public class Result<T>
{
    public bool Success { get; }
    public T Data { get; }
    public string Message { get; }
    public string[] ValidationErrors { get; }

    private Result(bool success, T data, string message, string[]? validationErrors = null)
    {
        Success = success;
        Data = data;
        Message = message;
        ValidationErrors = validationErrors ?? [];
    }

    public static Result<T> Succeeded(T value, string message = "Operation completed successfully")
        => new Result<T>(true, value, message);

    public static Result<T> Failed(string message)
        => new Result<T>(false, default!, message);

    public static Result<T?> ValidationFailure(string[]? validationErrors, string message = "Validation failed")
    {
        return new Result<T?>(false, default, message, validationErrors);
    }
}

public class Result
{
    public bool Success { get; }
    public string Message { get; }
    public string[] ValidationErrors { get; }

    private Result(bool success, string message, string[]? validationErrors = null)
    {
        Success = success;
        Message = message;
        ValidationErrors = validationErrors ?? Array.Empty<string>();
    }

    public static Result Succeeded(string message = "Operation completed successfully")
        => new Result(true, message);

    public static Result Failed(string message)
        => new Result(false, message);

    public static Result ValidationFailed(string[]? validationErrors, string message = "Validation failed")
        => new Result(false, message, validationErrors);
}