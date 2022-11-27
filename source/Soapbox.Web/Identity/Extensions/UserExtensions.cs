namespace Soapbox.Web.Identity.Extensions
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using IdentityModel;
    using Soapbox.Models;

    /// <summary>
    /// Provides extensions for the user/claims principal.
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Gets the user's identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns>The user's identifier.</returns>
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return GetUserId<string>(principal);
        }

        private static T GetUserId<T>(this ClaimsPrincipal principal)
        {
            if (principal is null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var loggedInUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(loggedInUserId, typeof(T));
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
            {
                return loggedInUserId != null
                    ? (T)Convert.ChangeType(loggedInUserId, typeof(T))
                    : (T)Convert.ChangeType(0, typeof(T));
            }
            else
            {
                throw new Exception("Invalid type provided");
            }
        }

        public static bool IsInRole(this ClaimsPrincipal principal, params UserRole[] roles)
        {
            var claimValue = principal.FindFirstValue(JwtClaimTypes.Role);
            if (!Enum.TryParse<UserRole>(claimValue, out var role))
            {
                return false;
            }

            return roles.Contains(role);
        }
    }
}
