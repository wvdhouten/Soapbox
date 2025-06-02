namespace Soapbox.Identity.Users.GetUser;

using Alkaline64.Injectable;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;

[Injectable]
public class GetUserHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public GetUserHandler(TransactionalUserManager<SoapboxUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<SoapboxUser>> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Error.NotFound($"User not found.");

        return user;
    }
}