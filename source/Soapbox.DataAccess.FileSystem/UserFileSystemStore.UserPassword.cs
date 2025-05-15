namespace Soapbox.DataAccess.FileSystem;

using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

public partial class UserFileSystemStore : IUserPasswordStore<SoapboxUser>
{
    public Task<bool> HasPasswordAsync(SoapboxUser user, CancellationToken cancellationToken)
        => Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    public Task<string?> GetPasswordHashAsync(SoapboxUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.PasswordHash);

    public Task SetPasswordHashAsync(SoapboxUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }
}
