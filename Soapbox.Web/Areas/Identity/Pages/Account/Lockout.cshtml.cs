namespace Soapbox.Web.Areas.Identity.Pages.Account
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}
