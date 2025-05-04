namespace Soapbox.Web.Controllers.Base;

using Microsoft.AspNetCore.Mvc;

public abstract class SoapboxControllerBase : Controller
{
    [TempData]
    public string StatusMessage { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }
}
