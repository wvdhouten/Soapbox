namespace Soapbox.Identity.Users.ListUsers;

using Soapbox.Application.Utils;
using Soapbox.Domain.Common;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Managers;

public class ListUsersHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;

    public ListUsersHandler(TransactionalUserManager<SoapboxUser> userManager)
    {
        _userManager = userManager;
    }

    public Result<IPagedList<SoapboxUser>> GetUsersPage(int page = 1, int pageSize = 25)
    {
        try
        {
            return Result.Success(_userManager.Users.OrderBy(u => u.UserName).GetPaged(page, pageSize));
        }
        catch
        {
            return Error.Unknown();
        }
    }
}
