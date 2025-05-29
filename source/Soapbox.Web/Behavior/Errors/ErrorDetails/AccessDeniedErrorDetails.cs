namespace Soapbox.Web.Behavior.Errors.ErrorDetails;
public record AccessDeniedErrorDetails : GenericErrorDetails
{
    public AccessDeniedErrorDetails(string requestId) : base(requestId)
    {
    }
}