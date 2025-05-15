namespace Soapbox.Web.Error;

using System;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Error.GetErrorDetails;

public class ErrorController : SoapboxControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index([FromServices] GetErrorDetailsHandler query, int? statusCode = null)
    {
        var result = query.Handle(statusCode);
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
