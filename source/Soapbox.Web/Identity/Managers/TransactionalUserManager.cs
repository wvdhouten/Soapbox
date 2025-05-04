namespace Soapbox.Web.Identity.Managers
{
    using System;
    using System.Collections.Generic;
    using Alkaline64.Injectable;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Soapbox.DataAccess.FileSystem.Abstractions;

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
        }

        public async Task ResetAsync()
        {
            await Store.ResetChangesAsync();
        }

        public async Task CommitAsync()
        {
            await Store.CommitChangesAsync();
        }
    }
}
