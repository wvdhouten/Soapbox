namespace Soapbox.DataAccess.FileSystem;

using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

public partial class UserFileSystemStore : IUserEmailStore<SoapboxUser>
{
    public Task SetEmailAsync(SoapboxUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(SoapboxUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.Email);

    public Task<bool> GetEmailConfirmedAsync(SoapboxUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.EmailConfirmed);

    public Task SetEmailConfirmedAsync(SoapboxUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<SoapboxUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        => Task.FromResult(Users.FirstOrDefault(user => user.NormalizedEmail == normalizedEmail));

    public Task<string?> GetNormalizedEmailAsync(SoapboxUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(SoapboxUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

}
