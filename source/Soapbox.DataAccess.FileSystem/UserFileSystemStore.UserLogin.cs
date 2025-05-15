namespace Soapbox.DataAccess.FileSystem;

using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

public partial class UserFileSystemStore : IUserLoginStore<SoapboxUser>
{
    public Task AddLoginAsync(SoapboxUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        if (!_memoryStore.LoginInfos.TryGetValue(user.Id, out var logins))
            _memoryStore.LoginInfos.Add(user.Id, logins ??= []);

        logins.Add(login);

        return Task.CompletedTask;
    }

    public Task RemoveLoginAsync(SoapboxUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        if (!_memoryStore.LoginInfos.TryGetValue(user.Id, out var logins))
            return Task.CompletedTask;

        var login = logins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
        if (login is not null)
            logins.Remove(login);

        if (logins.Count == 0)
            _memoryStore.LoginInfos.Remove(user.Id);

        return Task.CompletedTask;
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        _memoryStore.LoginInfos.TryGetValue(user.Id, out var logins);

        return Task.FromResult<IList<UserLoginInfo>>(logins ?? []);
    }

    public Task<SoapboxUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        var loginInfos = _memoryStore.LoginInfos.FirstOrDefault(x => x.Value.Any(info => info.LoginProvider == loginProvider && info.ProviderKey == providerKey));
        if (loginInfos.Equals(default(KeyValuePair<string, List<UserLoginInfo>>)))
            return Task.FromResult<SoapboxUser?>(null);

        _memoryStore.Users.TryGetValue(loginInfos.Key, out var user);
        return Task.FromResult(user);
    }
}
