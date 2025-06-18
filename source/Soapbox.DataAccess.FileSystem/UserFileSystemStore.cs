namespace Soapbox.DataAccess.FileSystem;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.DataAccess.Abstractions;
using Soapbox.DataAccess.FileSystem.Encryption;
using Soapbox.DataAccess.FileSystem.Identity;
using Soapbox.DataAccess.FileSystem.Serialization;
using Soapbox.Domain.Users;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

[Injectable<ITransactionalUserStore<SoapboxUser>>]
public partial class UserFileSystemStore :
    ITransactionalUserStore<SoapboxUser>,
    IProtectedUserStore<SoapboxUser>,
    IQueryableUserStore<SoapboxUser>
{
    private const string _userFileExtension = "sbuser";

    private readonly string _filePath = Path.Combine(Environment.CurrentDirectory, "Content", "Identity");

    private readonly MemoryStore _memoryStore;
    private readonly EncryptedFileHandler _fileHandler;

    public UserFileSystemStore(
        MemoryStore memoryStore,
        EncryptedFileHandler fileHandler)
    {
        _memoryStore = memoryStore;
        _fileHandler = fileHandler;
    }

    public IQueryable<SoapboxUser> Users => _memoryStore.Users.Values.AsQueryable();

    public async Task InitAsync()
    {
        _memoryStore.Users.Clear();
        _memoryStore.LoginInfos.Clear();
        _memoryStore.Authenticators.Clear();
        _memoryStore.RecoveryCodes.Clear();
        _memoryStore.Tokens.Clear();

        var files = Directory.GetFiles(_filePath, $"*.{_userFileExtension}");
        foreach (var file in files)
        {
            var json = await _fileHandler.ReadAllTextAsync(file);
            var user = JsonSerializer.Deserialize<UserRecord>(json, FileSystemSerialization.DefaultJsonSerializerOptions);

            if (user is null)
                continue;

            _memoryStore.Users.Add(user.User.Id, user.User);
            if (user.LoginInfos is not null)
                _memoryStore.LoginInfos.Add(user.User.Id, user.LoginInfos);
            if (user.AuthenticatorKey is not null)
                _memoryStore.Authenticators.Add(user.User.Id, user.AuthenticatorKey);
            if (user.RecoveryCodes is not null)
                _memoryStore.RecoveryCodes.Add(user.User.Id, user.RecoveryCodes);
            if (user.Tokens is not null)
                foreach (var token in user.Tokens)
                    _memoryStore.Tokens.Add((user.User.Id, token.Key.Provider, token.Key.Name), token.Value);
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
            RecoveryCodes = _memoryStore.RecoveryCodes.TryGetValue(user.Id, out var codes) ? codes : null,
            Tokens = _memoryStore.Tokens.Where(token => token.Key.UserId == user.Id).ToDictionary(token => (token.Key.Provider, token.Key.Name), token => token.Value)
        }, FileSystemSerialization.DefaultJsonSerializerOptions);

        await _fileHandler.WriteAllTextAsync(Path.Combine(_filePath, $"{user.Id}.{_userFileExtension}"), json);
    }

    private Task DestroyUserAsync(SoapboxUser user)
    {
        File.Delete(Path.Combine(_filePath, $"{user.Id}.{_userFileExtension}"));

        return Task.CompletedTask;
    }
}
