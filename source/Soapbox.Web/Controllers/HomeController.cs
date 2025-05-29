namespace Soapbox.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Controllers.Base;

public class HomeController : SoapboxControllerBase
{
    [HttpGet]
    public IActionResult Index() => View();
}
