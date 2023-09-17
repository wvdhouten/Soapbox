namespace Soapbox.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public abstract class SoapboxBaseController : Controller
    {

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }
    }
}
