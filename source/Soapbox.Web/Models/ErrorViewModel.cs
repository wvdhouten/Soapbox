namespace Soapbox.Web.Models;

public record ErrorViewModel
{
    public string RequestId { get; init; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string Message { get; set; }

    public string RequestedUrl { get; internal set; }

    public string RedirectUrl { get; internal set; }

    public ErrorViewModel(string requestId)
    {
        RequestId = requestId;
    }
}
