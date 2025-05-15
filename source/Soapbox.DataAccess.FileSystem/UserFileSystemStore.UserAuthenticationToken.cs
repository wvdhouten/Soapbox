namespace Soapbox.DataAccess.FileSystem;

using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

public partial class UserFileSystemStore : IUserAuthenticationTokenStore<SoapboxUser>
{
    public Task SetTokenAsync(SoapboxUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task RemoveTokenAsync(SoapboxUser user, string loginProvider, string name, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<string?> GetTokenAsync(SoapboxUser user, string loginProvider, string name, CancellationToken cancellationToken) => throw new NotImplementedException();
}
