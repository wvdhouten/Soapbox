namespace Soapbox.Domain.Results;

public record ValidationError : Error
{
    public Dictionary<string, string> Errors { get; init; } = [];

    internal ValidationError(string code, string message) : base(code, message)
    {
    }
}

public record ValidationError<TRequest> : ValidationError
{
    public TRequest Request { get; init; } = default!;

    internal ValidationError(TRequest request, string code, string message) : base(code, message)
    {
        Request = request;
    }
}
