namespace EduCare.Application.Helpers;

public enum RepositoryActionStatus
{
    Okay,
    Created,
    Updated,
    NotFound,
    ConcurrencyConflict,
    Deadlock,
    Deleted,
    NothingModified,
    Error,
    Invalid,        // FK constraint violation
    Canceled
}

public class RepositoryActionResult
{

}

public class RepositoryActionResult<T>(T? entity, RepositoryActionStatus status) : RepositoryActionResult
    where T : class
{
    public T? Entity { get; } = entity;
    public RepositoryActionStatus Status { get; } = status;
    public Exception? Exception { get; }

    public RepositoryActionResult(T? entity, RepositoryActionStatus status, Exception? exception) :
        this(entity, status)
    {
        Exception = exception;
    }
}