namespace Soapbox.Identity.Authenticators.AddAuthenticator;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;
using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

[Injectable]
public class AddAuthenticatorHandler
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UrlEncoder _urlEncoder;

    public AddAuthenticatorHandler(
        TransactionalUserManager<SoapboxUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _urlEncoder = urlEncoder;
    }

    public async Task<Result<AddAuthenticatorRequest>> CreateAddAuthenticatorRequestAsync(AddAuthenticatorRequest? request = null)
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user is null)
            return Error.NotFound("User not found.");

        var (sharedKey, authenticatorUri) = await LoadSharedKeyAndQrCodeUriAsync(user);

        request ??= new();
        request.SharedKey = sharedKey;
        request.AuthenticatorUri = authenticatorUri;

        return request;
    }

    public async Task<Result<string[]?>> AddAuthenticatorAsync(AddAuthenticatorRequest request)
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user is null)
            return Error.NotFound("User not found.");

        var verificationCode = request.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);
        if (!is2faTokenValid)
        {
            var (sharedKey, authenticatorUri) = await LoadSharedKeyAndQrCodeUriAsync(user);
            request.SharedKey = sharedKey;
            request.AuthenticatorUri = authenticatorUri;

            return Error.ValidationError(request, "Verification code is invalid.", new()
            {
                { "Input.Code", "Verification code is invalid." }
            });
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        if (await _userManager.CountRecoveryCodesAsync(user) != 0)
            return null!;

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return recoveryCodes?.ToArray();
    }

    private async Task<(string sharedKey, string authenticatorUri)> LoadSharedKeyAndQrCodeUriAsync(SoapboxUser user)
    {
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        if (string.IsNullOrEmpty(unformattedKey))
            throw new InvalidOperationException($"Unable to load authenticator key for user with ID '{user.Id}'.");

        return (sharedKey: FormatKey(unformattedKey), authenticatorUri: GenerateQrCodeUri(user.Id, unformattedKey));
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
            result.Append(unformattedKey[currentPosition..]);

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string userId, string unformattedKey)
        => string.Format(AuthenticatorUriFormat,
            _urlEncoder.Encode("Soapbox"),
            _urlEncoder.Encode(userId),
            unformattedKey);

}
