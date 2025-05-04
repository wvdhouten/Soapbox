namespace Soapbox.DataAccess.FileSystem;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.DataAccess.FileSystem.Abstractions;
using Soapbox.DataAccess.FileSystem.Identity;
using Soapbox.Domain.Users;

[Injectable<ITransactionalUserStore<SoapboxUser>>]
public class UserFileSystemStore :
    ITransactionalUserStore<SoapboxUser>,
    IUserEmailStore<SoapboxUser>,
    IUserPasswordStore<SoapboxUser>,
    IUserLoginStore<SoapboxUser>,
    IUserAuthenticatorKeyStore<SoapboxUser>,
    IUserTwoFactorStore<SoapboxUser>,
    IUserTwoFactorRecoveryCodeStore<SoapboxUser>,
    IProtectedUserStore<SoapboxUser>,
    IQueryableUserStore<SoapboxUser>
{
    private readonly string _filePath = Path.Combine(Environment.CurrentDirectory, "Content", "Identity");

    private readonly MemoryStore _memoryStore;

    public UserFileSystemStore(MemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public IQueryable<SoapboxUser> Users => _memoryStore.Users.Values.AsQueryable();

    public async Task InitAsync()
    {
        var files = Directory.GetFiles(_filePath, "*.sbu");
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file);
            var user = JsonSerializer.Deserialize<UserRecord>(json);

            if (user is null)
                continue;

            _memoryStore.Users.Add(user.User.Id, user.User);
            if (user.LoginInfos is not null)
                _memoryStore.LoginInfos.Add(user.User.Id, user.LoginInfos);
            if (user.AuthenticatorKey is not null)
                _memoryStore.Authenticators.Add(user.User.Id, user.AuthenticatorKey);
            if (user.RecoveryCodes is not null)
                _memoryStore.RecoveryCodes.Add(user.User.Id, user.RecoveryCodes);
        }
    }

    public async Task<IdentityResult> CreateAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        if (!_memoryStore.Users.TryAdd(user.Id, user))
            return IdentityResult.Failed(new IdentityError { Code = "UserExists", Description = "User already exists." });

        await PersistUserAsync(user);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        if (!_memoryStore.Users.Remove(user.Id))
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "User was not found." });

        await DestroyUserAsync(user);

        return IdentityResult.Success;
    }

    public Task<SoapboxUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        _memoryStore.Users.TryGetValue(userId, out var user);

        return Task.FromResult(user);
    }

    public Task<SoapboxUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = _memoryStore.Users.Values.FirstOrDefault(user => user.NormalizedUserName == normalizedUserName);

        return Task.FromResult(user);
    }

    public Task<string> GetUserIdAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(SoapboxUser user, string? userName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(SoapboxUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        _memoryStore.Users[user.Id] = user;
        await PersistUserAsync(user);

        return IdentityResult.Success;
    }

    public Task<bool> HasPasswordAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task<string?> GetPasswordHashAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task SetPasswordHashAsync(SoapboxUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task SetEmailAsync(SoapboxUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(SoapboxUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<SoapboxUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return Task.FromResult(Users.FirstOrDefault(user => user.NormalizedEmail == normalizedEmail));
    }

    public Task<string?> GetNormalizedEmailAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(SoapboxUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

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

    public Task SetTwoFactorEnabledAsync(SoapboxUser user, bool enabled, CancellationToken cancellationToken)
    {
        if (enabled)
            _memoryStore.RecoveryCodes[user.Id] = [];
        else
            _memoryStore.RecoveryCodes.Remove(user.Id);

        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(_memoryStore.RecoveryCodes.ContainsKey(user.Id));
    }

    public Task ReplaceCodesAsync(SoapboxUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        _memoryStore.RecoveryCodes[user.Id] = [.. recoveryCodes];

        return Task.CompletedTask;
    }

    public Task<bool> RedeemCodeAsync(SoapboxUser user, string code, CancellationToken cancellationToken)
    {
        if (!_memoryStore.RecoveryCodes.TryGetValue(user.Id, out var recoveryCodes))
            return Task.FromResult(false);

        if (!recoveryCodes.Contains(code))
            return Task.FromResult(false);

        recoveryCodes.Remove(code);

        return Task.FromResult(true);
    }

    public Task<int> CountCodesAsync(SoapboxUser user, CancellationToken cancellationToken)
    {
        if (!_memoryStore.RecoveryCodes.TryGetValue(user.Id, out var recoveryCodes))
            return Task.FromResult(0);

        return Task.FromResult(recoveryCodes.Count);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task ResetChangesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task CommitChangesAsync()
    {
        foreach(var user in _memoryStore.Users.Values)
        {
            await PersistUserAsync(user);
        }
    }

    private async Task PersistUserAsync(SoapboxUser user)
    {
        var json = JsonSerializer.Serialize(new UserRecord
        {
            User = user,
            LoginInfos = _memoryStore.LoginInfos.TryGetValue(user.Id, out var logins) ? logins : null,
            AuthenticatorKey = _memoryStore.Authenticators.TryGetValue(user.Id, out var key) ? key : null,
            RecoveryCodes = _memoryStore.RecoveryCodes.TryGetValue(user.Id, out var codes) ? codes : null
        });

        await File.WriteAllTextAsync(Path.Combine(_filePath, $"{user.Id}.user"), json);
    }

    private Task DestroyUserAsync(SoapboxUser user)
    {
        File.Delete(Path.Combine(_filePath, $"{user.Id}.user"));

        return Task.CompletedTask;
    }
}
