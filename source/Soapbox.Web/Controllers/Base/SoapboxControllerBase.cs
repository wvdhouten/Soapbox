namespace Soapbox.Web.Controllers.Base;

using Microsoft.AspNetCore.Mvc;

public abstract class SoapboxControllerBase : Controller
{
    [TempData, ViewData]
    public string? StatusMessage { get; set; }

    [TempData, ViewData]
    public string? ErrorMessage { get; set; }

    public ViewResult ValidationError<TModel>(string view, TModel model, Dictionary<string, string> errors)
    {
        foreach (var error in errors)
            ModelState.AddModelError(error.Key, error.Value);

        return View(view, model);
    }

    protected SoapboxControllerBase WithStatusMessage(string message)
    {
        StatusMessage = message;
        return this;
    }

    protected SoapboxControllerBase WithErrorMessage(string message)
    {
        ErrorMessage = message;
        return this;
    }

    protected SoapboxControllerBase WithViewData(string key, string value)
    {
        ViewData[key] = value;
        return this;
    }

    protected BadRequestObjectResult SomethingWentWrong() => BadRequest("Something went wrong.");
}
