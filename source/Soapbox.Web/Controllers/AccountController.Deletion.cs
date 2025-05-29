namespace Soapbox.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Identity.Deletion.DeleteAccount;
using Soapbox.Identity.Password.GetPasswordStatus;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> Delete(
        [FromServices] GetPasswordStatusHandler handler)
    {
        var result = await handler.GetPasswordStatusAsync();
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(new DeleteAccountRequest
            {
                RequirePassword = result.Value.HasPassword
            })
        };
    }

    [HttpPost]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteAccountHandler handler,
        [FromForm] DeleteAccountRequest request)
    {
        var result = await handler.DeleteAccount(request);

        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false when result.Error is ValidationError<DeleteAccountRequest> error => ValidationError(nameof(Delete), error.Request, error.Errors),
            false => BadRequest("Unable to process request."),
            _ => Redirect("~/")
        };
    }
}
