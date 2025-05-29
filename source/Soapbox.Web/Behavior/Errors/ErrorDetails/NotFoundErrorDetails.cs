namespace Soapbox.Web.Behavior.Errors.ErrorDetails;
public record NotFoundErrorDetails : GenericErrorDetails
{
    public string? RequestedUrl { get; internal set; }

    public string? RedirectUrl { get; internal set; }

    public NotFoundErrorDetails(string requestId) : base(requestId)
    {
    }
}
