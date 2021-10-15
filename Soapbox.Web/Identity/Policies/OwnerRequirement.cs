namespace Soapbox.Web.Identity.Policies
{
    using System;
    using System.Threading.Tasks;
    using IdentityModel;
    using Microsoft.AspNetCore.Authorization;
    using Soapbox.Core.Identity;
    using Soapbox.Models;

    public class OwnerAuthorizationHandler : AuthorizationHandler<OwnerRequirement, Post>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnerRequirement requirement, Post resource)
        {
            var roleClaim = context.User.FindFirst(JwtClaimTypes.Role);
            if (roleClaim is null)
            {
                return Task.CompletedTask;
            }

            var role = Enum.Parse(typeof(UserRole), roleClaim.Value);

            switch (role)
            {
                case UserRole.Administrator:
                case UserRole.Editor:
                    context.Succeed(requirement);
                    break;
                case UserRole.Author:
                case UserRole.Contributor:
                    if (context.User.Identity?.Name == resource.Author)
                    {
                        context.Succeed(requirement);
                    }
                    break;
            }

            return Task.CompletedTask;
        }
    }

    public class OwnerRequirement : IAuthorizationRequirement { }
}
