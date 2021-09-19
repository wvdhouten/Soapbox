namespace Soapbox.Web.Models.Account
{
    using Microsoft.AspNetCore.Mvc;

    public class RecoveryCodesModel
    {
        [TempData]
        public string[] RecoveryCodes { get; set; }
    }
}
