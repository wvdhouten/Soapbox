namespace Soapbox.DataAccess.FileSystem;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

[Injectable(Lifetime.Singleton)]
public class MemoryStore
{
    internal Dictionary<string, SoapboxUser> Users { get; } = [];

    internal Dictionary<string, List<UserLoginInfo>> LoginInfos { get; } = [];

    internal Dictionary<string, string> Authenticators { get; } = [];

    internal Dictionary<(string UserId, string Provider, string Name), string?> Tokens { get; } = [];

    internal Dictionary<string, List<string>> RecoveryCodes { get; } = [];
}
