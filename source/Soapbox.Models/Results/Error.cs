namespace Soapbox.Domain.Results;

public record Error(string Code, string Message)
{
    public static Error Unknown() => new(ErrorCode.Unknown, string.Empty);
    public static Error NullValue(string name) => new(ErrorCode.NullValue, $"{name} cannot be null.");
    public static Error NotFound(string message) => new(ErrorCode.NotFound, message);
    public static Error NotFound<TResult>(string message) => new(ErrorCode.NotFound, message);
    public static ValidationError ValidationError(string message, Dictionary<string, string> errors) => new(ErrorCode.Validation, message) { Errors = errors };
}
