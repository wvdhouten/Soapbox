namespace Soapbox.Domain.Results;

using System.Diagnostics.CodeAnalysis;

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        switch (isSuccess)
        {
            case true when error is not null:
                throw new InvalidOperationException();

            case false when error is null:
                throw new InvalidOperationException();

            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TResult> Success<TResult>(TResult? value) => new(value, true, null);
    public static Result<TResult> Failure<TResult>(Error error) => new(default, false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<TResult> : Result
{
    private readonly TResult? _value;

    protected internal Result(TResult? value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TResult Value => _value! ?? throw new InvalidOperationException("Result has no value");

    public static implicit operator Result<TResult>(TResult? value) => Success(value);
    public static implicit operator Result<TResult>(Error error) => Failure<TResult>(error);

    public static implicit operator TResult(Result<TResult> result) => result.IsSuccess
        ? result.Value
        : throw new InvalidOperationException($"Result is a failure: {result.Error!.Code}");
}
