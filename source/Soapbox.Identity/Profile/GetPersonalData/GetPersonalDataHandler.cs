namespace Soapbox.Identity.Profile.GetPersonalData;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;
using System.Security.Claims;

[Injectable]
public class GetPersonalDataHandler
{
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly ILogger<GetPersonalDataHandler> _logger;

    public GetPersonalDataHandler(TransactionalUserManager<SoapboxUser> userManager, ILogger<GetPersonalDataHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<Dictionary<string, string>>> GetPersonalDataAsync(ClaimsPrincipal claimPrincipal)
    {
        var userResult = await _userManager.GetUserAsync(claimPrincipal);
        if (userResult.IsFailure)
            return userResult.Error!;

        var user = userResult.Value;

        _logger.LogInformation("User with ID '{UserId}' requested their personal data.", user.Id);

        return await GetPersonalDataAsync(user);
    }

    public async Task<Dictionary<string, string>> GetPersonalDataAsync(SoapboxUser user)
    {
        var personalData = new Dictionary<string, string>();
        var personalDataProps = typeof(SoapboxUser).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
        foreach (var property in personalDataProps)
            personalData.Add(property.Name, property.GetValue(user)?.ToString() ?? "null");

        var logins = await _userManager.GetLoginsAsync(user);
        foreach (var login in logins)
            personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);

        return personalData;
    }
}
