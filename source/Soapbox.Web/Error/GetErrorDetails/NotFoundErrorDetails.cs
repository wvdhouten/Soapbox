namespace Soapbox.Web.Error.GetErrorDetails;
public record NotFoundErrorDetails : GenericErrorDetails
{
    public string? RequestedUrl { get; internal set; }

    public string? RedirectUrl { get; internal set; }

    public NotFoundErrorDetails(string requestId) : base(requestId)
    {
    }
}
