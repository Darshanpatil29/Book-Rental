namespace Bookify.Shared.Results;

/// <summary>
/// Represents the result of an operation, encapsulating success/failure along with optional error messages.
/// </summary>
public record Result(bool IsSuccess, string Error = "")
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T> Success<T>(T value) => new(true, "", value);

    /// <summary>
    /// Creates a failed result with an error message and no value.
    /// </summary>
    public static Result<T> Failure<T>(string error) => new(false, error);
}

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public record Result<T>(bool IsSuccess, string Error = "", T? Value = default)
{
    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T> Success(T value) => new(true, "", value);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static Result<T> Failure(string error) => new(false, error);

    /// <summary>
    /// Maps the current result to a new result by transforming the value.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> func)
    {
        if (!IsSuccess)
            return Result<TNew>.Failure(Error);

        return Result<TNew>.Success(func(Value!));
    }

    /// <summary>
    /// Chains operations that return Result{T}.
    /// </summary>
    public Result<TNew> FlatMap<TNew>(Func<T, Result<TNew>> func)
    {
        if (!IsSuccess)
            return Result<TNew>.Failure(Error);

        return func(Value!);
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);

        return this;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    public Result<T> OnFailure(Action<string> action)
    {
        if (!IsSuccess)
            action(Error);

        return this;
    }
}
