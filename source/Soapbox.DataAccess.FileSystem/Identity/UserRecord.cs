namespace Soapbox.DataAccess.FileSystem.Identity;

using Microsoft.AspNetCore.Identity;
using Soapbox.Domain.Users;

public record UserRecord
{
    public SoapboxUser User { get; set; } = null!;

    public List<UserLoginInfo>? LoginInfos { get; set; }

    public string? AuthenticatorKey { get; set; }

    public List<string>? RecoveryCodes { get; set; }

    public Dictionary<(string Provider, string Name), string?>? Tokens { get; set; }
}
