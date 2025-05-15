namespace Soapbox.Domain.Results;

public record ValidationError : Error
{
    public Dictionary<string, string> Errors { get; init; } = [];

    public ValidationError(string code, string message) : base(code, message)
    {
    }
}
