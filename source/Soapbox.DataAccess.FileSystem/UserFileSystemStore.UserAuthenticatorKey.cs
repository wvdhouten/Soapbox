namespace Soapbox.DataAccess.FileSystem;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

public partial class UserFileSystemStore : IUserAuthenticatorKeyStore<SoapboxUser>
{
    public Task SetAuthenticatorKeyAsync(SoapboxUser user, string key, CancellationToken cancellationToken)
    {
        _memoryStore.Authenticators[user.Id] = key;

        return Task.CompletedTask;
    }

    public Task<string?> GetAuthenticatorKeyAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        _memoryStore.Authenticators.TryGetValue(user.Id, out var key);

        return Task.FromResult(key);
    }
}
