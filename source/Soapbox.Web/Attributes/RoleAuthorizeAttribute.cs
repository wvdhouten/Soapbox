namespace Soapbox.Web.Attributes;

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Soapbox.Domain.Users;

public class RoleAuthorizeAttribute : ActionFilterAttribute
{
    private readonly UserRole[] _roles;

    public RoleAuthorizeAttribute(params UserRole[] roles)
    {
        _roles = roles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var roleClaim = context.HttpContext.User.FindFirst(ClaimTypes.Role);
        if (roleClaim is null)
        {
            context.Result = new RedirectToPageResult("/Account/Login");
            return;
        }

        var userRole = Enum.Parse<UserRole>(roleClaim.Value);
        if (_roles.Any(role => role == userRole))
            return;

        context.Result = new ForbidResult();
    }
}
