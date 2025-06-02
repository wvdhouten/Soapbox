namespace Soapbox.Identity.Users.EditUser;

using Alkaline64.Injectable;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;

[Injectable]
public class EditUserHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public EditUserHandler(TransactionalUserManager<SoapboxUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> EditUserAsync(EditUserRequest request)
    {
        var existing = await _userManager.FindByIdAsync(request.Id!);
        if (existing is null)
            return Error.NotFound($"User not found.");

        existing.UserName = request.UserName;
        existing.Email = request.Email;
        existing.DisplayName = request.DisplayName;
        existing.Role = request.Role;

        var result = await _userManager.UpdateAsync(existing);
        if (!result.Succeeded)
            return Error.ValidationError("Failed to update user", result.Errors.ToDictionary(e => e.Code, e => e.Description));

        await _userManager.CommitAsync();

        return Result.Success();
    }
}
