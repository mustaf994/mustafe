namespace ReExamManagementSystem.Application.Common;

/// <summary>
/// Uniform outcome for a service write operation, so controllers have one
/// consistent shape to branch on instead of exceptions-for-control-flow or
/// ad hoc tuples.
/// </summary>
public class ServiceResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ServiceResult Success() => new() { Succeeded = true };
    public static ServiceResult Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };
    public new static ServiceResult<T> Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
}
