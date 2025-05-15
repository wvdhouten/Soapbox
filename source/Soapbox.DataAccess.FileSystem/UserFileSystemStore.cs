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
public partial class UserFileSystemStore :
    ITransactionalUserStore<SoapboxUser>,
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
        _memoryStore.Users.Clear();
        _memoryStore.LoginInfos.Clear();
        _memoryStore.Authenticators.Clear();
        _memoryStore.RecoveryCodes.Clear();

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
        => Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(SoapboxUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.UserName);

    public Task SetUserNameAsync(SoapboxUser user, string? userName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(SoapboxUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.NormalizedUserName);

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

    public void Dispose()
        => GC.SuppressFinalize(this);

    public async Task ResetChangesAsync()
        => await InitAsync();

    public async Task CommitChangesAsync()
    {
        foreach (var user in _memoryStore.Users.Values)
            await PersistUserAsync(user);
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
