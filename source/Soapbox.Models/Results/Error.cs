namespace Soapbox.Domain.Results;

public record Error
{
    public string Code { get; init; } = ErrorCode.Unknown;

    public string Message { get; init; } = string.Empty;

    protected Error() { }

    protected Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error Unknown(string message = "") => new(ErrorCode.Unknown, message);
    public static Error Other(string code, string message = "") => new(code, message);
    public static Error NullValue(string name) => new(ErrorCode.NullValue, $"{name} cannot be null.");
    public static Error NotFound(string message) => new(ErrorCode.NotFound, message);
    public static Error NotFound<TResult>(string message) => new(ErrorCode.NotFound, message);
    public static Error InvalidOperation(string message) => new(ErrorCode.InvalidOperation, message);
    public static ValueError<TValue> ValueError<TValue>(TValue value, string code, string message) => new(value, code, message);
    public static ValidationError ValidationError(string message, Dictionary<string, string> errors) => new(ErrorCode.Validation, message) { Errors = errors };
    public static ValidationError<TRequest> ValidationError<TRequest>(TRequest request, string message, Dictionary<string, string> errors) => new(request, ErrorCode.Validation, message) { Errors = errors };
}
