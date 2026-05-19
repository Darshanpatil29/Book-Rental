namespace Bookify.Application.Shared;

/// <summary>
/// Result type for operations that can succeed or fail
/// Provides a clean way to handle errors without exceptions
/// </summary>
public abstract record Result
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new SuccessResult();

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    public static Result Failure(string errorMessage) => new FailureResult(errorMessage);

    public sealed record SuccessResult : Result
    {
        public SuccessResult() : base(true, null) { }
    }

    public sealed record FailureResult : Result
    {
        public FailureResult(string errorMessage) : base(false, errorMessage) { }
    }
}

/// <summary>
/// Generic result type for operations that return a value on success
/// </summary>
/// <typeparam name="T">Type of value returned on success</typeparam>
public abstract record Result<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Value returned on success (default if failed)
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Optional error code for specific error handling
    /// </summary>
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, T? value, string? errorMessage, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<T> Success(T value) => new SuccessResult(value);

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    public static Result<T> Failure(string errorMessage, string? errorCode = null)
        => new FailureResult(errorMessage, errorCode);

    /// <summary>
    /// Transforms the success value using a mapping function
    /// If result is failure, returns failure with same error
    /// </summary>
    public Result<TNext> Map<TNext>(Func<T, TNext> mapper)
    {
        if (!IsSuccess)
        {
            return Result<TNext>.Failure(ErrorMessage!, ErrorCode);
        }

        return Result<TNext>.Success(mapper(Value!));
    }

    /// <summary>
    /// Chains another operation that returns Result<TNext>
    /// If this result is failure, returns failure without executing next operation
    /// </summary>
    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> binder)
    {
        if (!IsSuccess)
        {
            return Result<TNext>.Failure(ErrorMessage!, ErrorCode);
        }

        return binder(Value!);
    }

    /// <summary>
    /// Executes an action if the operation was successful
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Value!);
        }

        return this;
    }

    /// <summary>
    /// Executes an action if the operation failed
    /// </summary>
    public Result<T> OnFailure(Action<string> action)
    {
        if (!IsSuccess)
        {
            action(ErrorMessage!);
        }

        return this;
    }

    public sealed record SuccessResult : Result<T>
    {
        public SuccessResult(T value) : base(true, value, null, null) { }
    }

    public sealed record FailureResult : Result<T>
    {
        public FailureResult(string errorMessage, string? errorCode = null)
            : base(false, default, errorMessage, errorCode) { }
    }
}
