namespace Soapbox.Identity.Custom;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using System;
using System.Collections.Generic;
using System.Security.Claims;

[Injectable]
public class TransactionalUserManager<TUser> : UserManager<TUser> where TUser : class
{
    protected internal new ITransactionalUserStore<TUser> Store { get; set; }

    public TransactionalUserManager(ITransactionalUserStore<TUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<TUser> passwordHasher,
        IEnumerable<IUserValidator<TUser>> userValidators,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<TransactionalUserManager<TUser>> logger)
        : base(store,
              optionsAccessor,
              passwordHasher,
              userValidators,
              passwordValidators,
              keyNormalizer,
              errors,
              services,
              logger)
    {
        Store = store;
    }

    public async Task ResetAsync() => await Store.ResetChangesAsync();

    public async Task CommitAsync() => await Store.CommitChangesAsync();

    public new async Task<Result<TUser>> GetUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        var user = await base.GetUserAsync(claimsPrincipal);
        return user is not null
            ? user
            : Error.NotFound($"Unable to find user with ID '{GetUserId(claimsPrincipal)}'.");
    }
}
