namespace Soapbox.DataAccess.FileSystem.Abstractions;

using Microsoft.AspNetCore.Identity;

public interface ITransactionalUserStore<TUser> : IUserStore<TUser> where TUser : class
{
    public Task InitAsync();

    public Task ResetChangesAsync();

    public Task CommitChangesAsync();
}
