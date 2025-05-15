namespace Soapbox.Web.Common;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Common.Base;

public class HomeController : SoapboxControllerBase
{
    [HttpGet]
    public IActionResult Index() => View();
}
