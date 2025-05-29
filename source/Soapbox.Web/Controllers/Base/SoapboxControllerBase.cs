namespace Soapbox.Web.Controllers.Base;

using Microsoft.AspNetCore.Mvc;

public abstract class SoapboxControllerBase : Controller
{
    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public ViewResult ValidationError<TModel>(string view, TModel model, Dictionary<string, string> errors)
    {
        foreach (var error in errors)
            ModelState.AddModelError(error.Key, error.Value);

        return View(view, model);
    }

    public SoapboxControllerBase WithStatusMessage(string message)
    {
        StatusMessage = message;
        return this;
    }

    public SoapboxControllerBase WithErrorMessage(string message)
    {
        ErrorMessage = message;
        return this;
    }

    public SoapboxControllerBase WithViewData(string key, string value)
    {
        ViewData[key] = value;
        return this;
    }
}
