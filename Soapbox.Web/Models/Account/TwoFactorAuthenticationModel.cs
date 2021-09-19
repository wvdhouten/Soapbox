namespace Soapbox.Web.Models.Account
{
    using Microsoft.AspNetCore.Mvc;

    public class TwoFactorAuthenticationModel
    {
        public bool HasAuthenticator { get; set; }

        public int RecoveryCodesLeft { get; set; }

        [BindProperty]
        public bool Is2faEnabled { get; set; }

        public bool IsMachineRemembered { get; set; }
    }
}
