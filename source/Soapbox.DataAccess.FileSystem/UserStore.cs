namespace Soapbox.DataAccess.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Soapbox.Models;

    public class UserStore : IUserStore<SoapboxUser>, IUserPasswordStore<SoapboxUser>, IUserEmailStore<SoapboxUser>, IQueryableUserStore<SoapboxUser>
    {
        private readonly string _filePath = Path.Combine(Environment.CurrentDirectory, "Content", "users.json");
        private IList<SoapboxUser> _users;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            Converters =
            {

            }
        };

        public IQueryable<SoapboxUser> Users
        {
            get
            {
                if (_users == null)
                {
                    LoadUsers();
                }

                return _users.AsQueryable();
            }
        }

        private void LoadUsers()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _users = JsonSerializer.Deserialize<List<SoapboxUser>>(json);
            }
            else
            {
                _users = _users = new List<SoapboxUser>();
            }
        }

        private async Task SaveUsers(CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(_users);
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }

        public async Task<IdentityResult> CreateAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            LoadUsers();
            _users.Add(user);
            await SaveUsers(cancellationToken);

            return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<SoapboxUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.FirstOrDefault(user => user.Id == userId));
        }

        public Task<SoapboxUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.FirstOrDefault(user => user.NormalizedUserName == normalizedUserName));
        }

        public Task<string> GetNormalizedUserNameAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(SoapboxUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(SoapboxUser user, string userName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = userName;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> UpdateAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(SoapboxUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public Task SetEmailAsync(SoapboxUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(SoapboxUser user, CancellationToken cancellationToken)
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

        public Task<SoapboxUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.FirstOrDefault(user => user.NormalizedEmail == normalizedEmail));
        }

        public Task<string> GetNormalizedEmailAsync(SoapboxUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(SoapboxUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }
    }
}
