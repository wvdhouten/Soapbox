namespace Soapbox.Web.Helpers;

using System;
using System.Linq;
using System.Security.Claims;
using Soapbox.Domain.Users;

public static class UserClaimExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
        => principal.GetUserId<string>();

    private static TId GetUserId<TId>(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var loggedInUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User ID not found in claims.");

        if (typeof(TId) == typeof(string))
            return (TId)Convert.ChangeType(loggedInUserId, typeof(TId));
        else if (typeof(TId) == typeof(int) || typeof(TId) == typeof(long))
            return loggedInUserId != null
                ? (TId)Convert.ChangeType(loggedInUserId, typeof(TId))
                : (TId)Convert.ChangeType(0, typeof(TId));
        else if (typeof(TId) == typeof(Guid))
            return (TId)Convert.ChangeType(loggedInUserId, typeof(TId));
        else
            throw new InvalidOperationException("Invalid user ID type provided.");
    }

    public static bool IsInRole(this ClaimsPrincipal principal, params UserRole[] roles)
    {
        var claimValue = principal.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<UserRole>(claimValue, out var role)
            && roles.Contains(role);
    }
}
