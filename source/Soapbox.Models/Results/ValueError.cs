namespace Soapbox.Domain.Results;
public record ValueError<TValue> : Error
{
    public TValue Value { get; init; }

    public ValueError(TValue value, string code, string message) : base(code, message)
    {
        Value = value;
    }
}
