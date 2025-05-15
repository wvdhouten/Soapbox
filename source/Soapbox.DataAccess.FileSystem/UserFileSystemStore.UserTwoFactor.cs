namespace Soapbox.DataAccess.FileSystem;

using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

public partial class UserFileSystemStore :
    IUserTwoFactorStore<SoapboxUser>,
    IUserTwoFactorRecoveryCodeStore<SoapboxUser>
{
    public Task SetTwoFactorEnabledAsync(SoapboxUser user, bool enabled, CancellationToken cancellationToken)
    {
        if (enabled)
            _memoryStore.RecoveryCodes[user.Id] = [];
        else
            _memoryStore.RecoveryCodes.Remove(user.Id);

        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(SoapboxUser user, CancellationToken cancellationToken)
        => Task.FromResult(_memoryStore.RecoveryCodes.ContainsKey(user.Id));

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
        => _memoryStore.RecoveryCodes.TryGetValue(user.Id, out var recoveryCodes)
        ? Task.FromResult(recoveryCodes.Count)
        : Task.FromResult(0);
}
