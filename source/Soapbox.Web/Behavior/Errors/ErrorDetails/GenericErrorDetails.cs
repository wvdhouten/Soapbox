namespace Soapbox.Web.Behavior.Errors.ErrorDetails;

public record GenericErrorDetails
{
    public string RequestId { get; init; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string Message { get; set; } = string.Empty!;

    public GenericErrorDetails(string requestId)
    {
        RequestId = requestId;
    }
}
