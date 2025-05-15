namespace Soapbox.Web.Account;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Web.Account.Password.ChangePassword;
using Soapbox.Web.Account.Password.GetPasswordStatus;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> ChangePassword(
        [FromServices] GetPasswordStatusQuery query)
    {
        var result = await query.HandleAsync();
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(new ChangePasswordRequest { HasPassword = result.Value.HasPassword })
        };
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(
        [FromServices] ChangePasswordCommand command,
        [FromForm] ChangePasswordRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await command.HandleAsync(model);
        switch (result.IsSuccess)
        {
            case false when result.Error?.Code == ErrorCode.NotFound:
                return NotFound(result.Error.Message);
            case false when result.Error is ValidationError validationError:
                foreach (var error in validationError.Errors)
                    ModelState.AddModelError(string.Empty, error.Value);
                return View(model);
            case false:
                return BadRequest("Unable to process request.");
            default:
                StatusMessage = "Your password has been changed.";
                return RedirectToAction(nameof(Index));
        }
    }
}
