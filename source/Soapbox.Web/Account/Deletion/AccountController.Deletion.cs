namespace Soapbox.Web.Account;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Web.Account.Password.GetPasswordStatus;
using Soapbox.Web.Models.Account;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> Delete(
        [FromServices] GetPasswordStatusQuery query)
    {
        var result = await query.HandleAsync();
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(new DeleteModel
            {
                RequirePassword = result.Value.HasPassword
            })
        };
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromForm] DeleteModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        model.RequirePassword = await _userManager.HasPasswordAsync(user);
        if (model.RequirePassword && !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Incorrect password.");
            return View(model);
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

        return Redirect("~/");
    }
}
