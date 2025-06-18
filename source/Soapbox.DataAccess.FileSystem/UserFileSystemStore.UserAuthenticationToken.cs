namespace Soapbox.DataAccess.FileSystem;

using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

public partial class UserFileSystemStore : IUserAuthenticationTokenStore<SoapboxUser>
{
    public Task SetTokenAsync(SoapboxUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken)
    {
        _memoryStore.Tokens.Add((user.Id, loginProvider, name), value);

        return Task.CompletedTask;
    }

    public Task RemoveTokenAsync(SoapboxUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        _memoryStore.Tokens.Remove((user.Id, loginProvider, name));

        return Task.CompletedTask;
    }

    public Task<string?> GetTokenAsync(SoapboxUser user, string loginProvider, string name, CancellationToken cancellationToken)
        => Task.FromResult(_memoryStore.Tokens.TryGetValue((user.Id, loginProvider, name), out var value)
            ? value
            : null);
}
