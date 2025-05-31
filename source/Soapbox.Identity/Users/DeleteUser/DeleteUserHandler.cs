namespace Soapbox.Identity.Users.DeleteUser;

using Microsoft.AspNetCore.Http;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

public class DeleteUserHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteUserHandler(TransactionalUserManager<SoapboxUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Error.NotFound("User not found.");

        var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
        if (user.Id == userId)
            return Error.InvalidOperation("You cannot delete yourself.");

        await _userManager.DeleteAsync(user);

        return Result.Success();
    }
}
