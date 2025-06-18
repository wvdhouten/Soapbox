namespace Soapbox.Identity.Custom;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

/// <summary>
/// A custom password hasher that prefixes the user ID to the password before hashing, to avoid password-hash portability.
/// </summary>
[Injectable<IPasswordHasher<SoapboxUser>>]
public class UniquePasswordHasher : PasswordHasher<SoapboxUser>
{
    public override string HashPassword(SoapboxUser user, string password)
        => base.HashPassword(user, $"{user.Id}-{password}");

    public override PasswordVerificationResult VerifyHashedPassword(SoapboxUser user, string hashedPassword, string providedPassword)
        => base.VerifyHashedPassword(user, hashedPassword, $"{user.Id}-{providedPassword}");
}
