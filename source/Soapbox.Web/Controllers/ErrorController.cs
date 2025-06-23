namespace Soapbox.Web.Controllers;
using System;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Behavior.Errors;
using Soapbox.Web.Behavior.Errors.ErrorDetails;
using Soapbox.Web.Controllers.Base;

public class ErrorController : SoapboxControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index(
        [FromServices] GetErrorDetailsHandler handler,
        [FromQuery] int? statusCode = null)
    {
        var result = handler.GetErrorDetails(statusCode);
        return result.IsSuccess switch
        {
            true when result.Value is NotFoundErrorDetails => View("NotFound", result.Value),
            true when result.Value is AccessDeniedErrorDetails => View("AccessDenied", result.Value),
            true when result.Value is not null => View("Error", result.Value),
            _ => throw new InvalidOperationException("Failed to obtain error details.")
        };
    }

    [HttpGet]
    public IActionResult Offline() => View();
}
