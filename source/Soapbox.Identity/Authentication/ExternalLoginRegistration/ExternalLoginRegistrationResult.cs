namespace Soapbox.Identity.Authentication.ExternalLoginRegistration;

using Soapbox.Domain.Results;

public class ExternalLoginRegistrationResult : Result
{
    public bool RequiresConfirmation { get; }

    public ExternalLoginRegistrationResult(bool requiresConfirmation) : base(true, null)
    {
        RequiresConfirmation = requiresConfirmation;
    }
}
