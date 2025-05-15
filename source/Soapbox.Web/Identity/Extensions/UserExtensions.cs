namespace Soapbox.Web.Identity.Extensions
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using IdentityModel;
    using Soapbox.Domain.Users;

    public static class UserExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return GetUserId<string>(principal);
        }

        private static TId GetUserId<TId>(this ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal);

            var loggedInUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("User ID not found in claims");

            // TODO: Add Guid support
            if (typeof(TId) == typeof(string))
                return (TId)Convert.ChangeType(loggedInUserId, typeof(TId));
            else if (typeof(TId) == typeof(int) || typeof(TId) == typeof(long))
                return loggedInUserId != null
                    ? (TId)Convert.ChangeType(loggedInUserId, typeof(TId))
                    : (TId)Convert.ChangeType(0, typeof(TId));
            else
                throw new Exception("Invalid type provided");
        }

        public static bool IsInRole(this ClaimsPrincipal principal, params UserRole[] roles)
        {
            var claimValue = principal.FindFirstValue(JwtClaimTypes.Role);
            if (!Enum.TryParse<UserRole>(claimValue, out var role))
                return false;

            return roles.Contains(role);
        }
    }
}
