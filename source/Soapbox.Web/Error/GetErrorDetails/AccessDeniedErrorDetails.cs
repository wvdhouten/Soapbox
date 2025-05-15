namespace Soapbox.Web.Error.GetErrorDetails;
public record AccessDeniedErrorDetails : GenericErrorDetails
{
    public AccessDeniedErrorDetails(string requestId) : base(requestId)
    {
    }
}